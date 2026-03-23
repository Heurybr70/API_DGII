using System.Text;
using System.Text.Json;
using DgiiSaas.Application.Interfaces;
using DgiiSaas.Domain.Entities;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace DgiiSaas.Infrastructure.Services;

public class WebhookService : IWebhookService
{
    private readonly IWebhookRepository _webhookRepo;
    private readonly IBackgroundJobClient _backgroundJobs;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(
        IWebhookRepository webhookRepo,
        IBackgroundJobClient backgroundJobs,
        ILogger<WebhookService> logger)
    {
        _webhookRepo = webhookRepo;
        _backgroundJobs = backgroundJobs;
        _logger = logger;
    }

    public async Task EnqueueWebhookAsync(Guid tenantId, string eventType, object payload, CancellationToken ct = default)
    {
        _logger.LogInformation("Encolando webhook para Tenant {TenantId}, Evento {EventType}", tenantId, eventType);

        var subscriptions = await _webhookRepo.GetActiveByTenantAsync(tenantId, ct);
        
        // Filter subscriptions interested in this event type
        var activeSubs = subscriptions.Where(s => s.Events.Contains(eventType) || s.Events == "*").ToList();

        if (!activeSubs.Any())
        {
            _logger.LogInformation("No hay suscripciones activas para el evento {EventType}", eventType);
            return;
        }

        var jsonPayload = JsonSerializer.Serialize(payload);

        foreach (var sub in activeSubs)
        {
            var delivery = new WebhookDelivery
            {
                WebhookSubscriptionId = sub.Id,
                EventType = eventType,
                Payload = jsonPayload,
                AttemptNumber = 1,
                IsSuccess = false
            };

            await _webhookRepo.AddDeliveryAsync(delivery, ct);
            await _webhookRepo.SaveChangesAsync(ct);

            // Enqueue Hangfire Job
            _backgroundJobs.Enqueue<WebhookDeliveryJob>(job => job.DeliverAsync(delivery.Id, ct));
        }
    }
}

public class WebhookDeliveryJob
{
    private readonly IWebhookRepository _webhookRepo;
    private readonly ITenantRepository _tenantRepo;
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookDeliveryJob> _logger;

    public WebhookDeliveryJob(
        IWebhookRepository webhookRepo,
        ITenantRepository tenantRepo,
        HttpClient httpClient,
        ILogger<WebhookDeliveryJob> logger)
    {
        _webhookRepo = webhookRepo;
        _tenantRepo = tenantRepo;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task DeliverAsync(Guid deliveryId, CancellationToken ct)
    {
        // To be fully implemented: fetch delivery, send HTTP POST, update status, schedule retry if failed.
        _logger.LogInformation("Procesando webhook delivery {DeliveryId}", deliveryId);
        
        // Simulating the delivery
        await Task.Delay(100, ct);
    }
}
