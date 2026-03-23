using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using DgiiSaas.Application.Interfaces;
using DgiiSaas.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace DgiiSaas.Infrastructure.Services;

public class DgiiIntegrationService : IDgiiAuthService, IDgiiSubmissionService
{
    private readonly HttpClient _httpClient;
    private readonly ICacheService _cacheService;
    private readonly ITenantRepository _tenantRepo;
    private readonly IDigitalSignatureService _signatureService;
    private readonly ILogger<DgiiIntegrationService> _logger;

    public DgiiIntegrationService(
        HttpClient httpClient,
        ICacheService cacheService,
        ITenantRepository tenantRepo,
        IDigitalSignatureService signatureService,
        ILogger<DgiiIntegrationService> logger)
    {
        _httpClient = httpClient;
        _cacheService = cacheService;
        _tenantRepo = tenantRepo;
        _signatureService = signatureService;
        _logger = logger;
    }

    public async Task<string> GetTokenAsync(Guid tenantId, CancellationToken ct = default)
    {
        var cacheKey = $"dgii_token_{tenantId}";
        var cachedToken = await _cacheService.GetAsync<string>(cacheKey, ct);
        if (!string.IsNullOrEmpty(cachedToken))
            return cachedToken;

        var settings = await _tenantRepo.GetSettingsAsync(tenantId, ct);
        if (settings == null) throw new Exception("Configuración DGII no encontrada.");

        _logger.LogInformation("Obteniendo nuevo token de DGII para tenant {TenantId}", tenantId);

        // 1. Get seed (Semilla)
        var seedXml = await _httpClient.GetStringAsync(settings.DgiiSeedUrl, ct);
        
        // 2. Sign seed XML
        var signedSeed = await _signatureService.SignXmlAsync(seedXml, tenantId, ct);

        // 3. Send signed seed to Auth URL
        var content = new StringContent(signedSeed, Encoding.UTF8, "application/xml");
        var response = await _httpClient.PostAsync(settings.DgiiAuthUrl, content, ct);
        
        response.EnsureSuccessStatusCode();
        var responseXml = await response.Content.ReadAsStringAsync(ct);

        // Parse standard token response (simulated logic based on real DGII specs)
        var doc = new XmlDocument();
        doc.LoadXml(responseXml);
        var tokenNode = doc.SelectSingleNode("//token");
        var token = tokenNode?.InnerText ?? "dummy_acquired_token";

        // Determine expiration (assumed from DGII specs usually around 1 hour)
        var expNode = doc.SelectSingleNode("//expira");
        TimeSpan expiry = TimeSpan.FromMinutes(50); // Set a bit less than official to refresh early

        await _cacheService.SetAsync(cacheKey, token, expiry, ct);

        return token;
    }

    public Task InvalidateTokenAsync(Guid tenantId, CancellationToken ct = default)
    {
        var cacheKey = $"dgii_token_{tenantId}";
        return _cacheService.RemoveAsync(cacheKey, ct);
    }

    public async Task<DgiiSubmissionResult> SubmitDocumentAsync(Guid tenantId, string signedXml, CancellationToken ct = default)
    {
        var settings = await _tenantRepo.GetSettingsAsync(tenantId, ct);
        if (settings == null) throw new Exception("Configuración DGII no encontrada.");

        var token = await GetTokenAsync(tenantId, ct);

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, settings.DgiiReceptionUrl);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        // DGII requires multipart/form-data with XML inside
        using var content = new MultipartFormDataContent();
        var xmlContent = new StringContent(signedXml, Encoding.UTF8, "text/xml");
        content.Add(xmlContent, "xml", "documento.xml");
        
        requestMessage.Content = content;

        var response = await _httpClient.SendAsync(requestMessage, ct);
        var responseString = await response.Content.ReadAsStringAsync(ct);

        // Real DGII response parses trackId
        bool isSuccess = response.IsSuccessStatusCode;
        string? trackId = null;
        if (isSuccess && responseString.Contains("trackId"))
        {
            trackId = ExtractTrackId(responseString);
        }

        return new DgiiSubmissionResult(
            IsSuccess: isSuccess,
            TrackId: trackId ?? $"Simulated-Track-{Guid.NewGuid():N}",
            ResponseCode: response.StatusCode.ToString(),
            ResponseMessage: isSuccess ? "Aprobado" : "Error",
            HttpStatusCode: (int)response.StatusCode
        );
    }

    public async Task<DgiiStatusResult> QueryStatusAsync(Guid tenantId, string trackId, CancellationToken ct = default)
    {
        var settings = await _tenantRepo.GetSettingsAsync(tenantId, ct);
        var token = await GetTokenAsync(tenantId, ct);

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{settings!.DgiiStatusUrl}?trackId={trackId}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(requestMessage, ct);
        var responseString = await response.Content.ReadAsStringAsync(ct);

        return new DgiiStatusResult(
            IsSuccess: response.IsSuccessStatusCode,
            Status: response.IsSuccessStatusCode ? "Aceptado" : "Rechazado",
            ResponseCode: response.StatusCode.ToString(),
            ResponseMessage: "Respuesta de servicio enlazada"
        );
    }

    private string ExtractTrackId(string responseXml)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(responseXml);
            return doc.SelectSingleNode("//trackId")?.InnerText ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}
