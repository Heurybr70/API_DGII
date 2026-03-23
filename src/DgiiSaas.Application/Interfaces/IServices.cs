using DgiiSaas.Domain.Entities;

namespace DgiiSaas.Application.Interfaces;

/// <summary>
/// Contexto del tenant actual (resuelto desde request).
/// </summary>
public interface ITenantContext
{
    Guid TenantId { get; }
    Tenant? CurrentTenant { get; }
    Task<Tenant> GetCurrentTenantAsync(CancellationToken ct = default);
}

/// <summary>
/// Servicio de autenticación con DGII.
/// </summary>
public interface IDgiiAuthService
{
    Task<string> GetTokenAsync(Guid tenantId, CancellationToken ct = default);
    Task InvalidateTokenAsync(Guid tenantId, CancellationToken ct = default);
}

/// <summary>
/// Servicio de envío y consulta a DGII.
/// </summary>
public interface IDgiiSubmissionService
{
    Task<DgiiSubmissionResult> SubmitDocumentAsync(Guid tenantId, string signedXml, CancellationToken ct = default);
    Task<DgiiStatusResult> QueryStatusAsync(Guid tenantId, string trackId, CancellationToken ct = default);
}

/// <summary>
/// Generador de XML e-CF desde modelo canónico.
/// </summary>
public interface IXmlGeneratorService
{
    Task<string> GenerateXmlAsync(ElectronicDocument document, CancellationToken ct = default);
}

/// <summary>
/// Servicio de firma digital XML.
/// </summary>
public interface IDigitalSignatureService
{
    Task<string> SignXmlAsync(string xml, Guid tenantId, CancellationToken ct = default);
}

/// <summary>
/// Caché distribuida.
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
}

/// <summary>
/// Lock distribuido.
/// </summary>
public interface IDistributedLockService
{
    Task<IAsyncDisposable?> AcquireLockAsync(string resource, TimeSpan expiry, CancellationToken ct = default);
}

/// <summary>
/// Servicio de webhook.
/// </summary>
public interface IWebhookService
{
    Task EnqueueWebhookAsync(Guid tenantId, string eventType, object payload, CancellationToken ct = default);
}

/// <summary>
/// Almacenamiento de archivos.
/// </summary>
public interface IFileStorageService
{
    Task<string> SaveFileAsync(Guid tenantId, string fileName, byte[] content, CancellationToken ct = default);
    Task<byte[]?> GetFileAsync(string filePath, CancellationToken ct = default);
}

// --- Result DTOs ---
public record DgiiSubmissionResult(
    bool IsSuccess,
    string? TrackId,
    string? ResponseCode,
    string? ResponseMessage,
    int HttpStatusCode);

public record DgiiStatusResult(
    bool IsSuccess,
    string Status,
    string? ResponseCode,
    string? ResponseMessage);
