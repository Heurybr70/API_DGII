using DgiiSaas.Domain.Entities;
using DgiiSaas.Domain.Enums;

namespace DgiiSaas.Application.Interfaces;

/// <summary>
/// Repositorio de documentos electrónicos.
/// </summary>
public interface IDocumentRepository
{
    Task<ElectronicDocument?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ElectronicDocument?> GetByIdempotencyKeyAsync(Guid tenantId, string key, CancellationToken ct = default);
    Task<ElectronicDocument?> GetByEncfAsync(Guid tenantId, string encf, CancellationToken ct = default);
    Task<IReadOnlyList<ElectronicDocument>> GetByStatusAsync(Guid tenantId, DocumentStatus status, int maxCount = 50, CancellationToken ct = default);
    Task<IReadOnlyList<ElectronicDocument>> GetPendingSubmissionAsync(int maxCount = 50, CancellationToken ct = default);
    Task<IReadOnlyList<ElectronicDocument>> GetPendingStatusCheckAsync(int maxCount = 50, CancellationToken ct = default);
    Task AddAsync(ElectronicDocument document, CancellationToken ct = default);
    Task UpdateAsync(ElectronicDocument document, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

/// <summary>
/// Repositorio de tenants.
/// </summary>
public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Tenant?> GetByRncAsync(string rnc, CancellationToken ct = default);
    Task<TenantSettings?> GetSettingsAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(Tenant tenant, CancellationToken ct = default);
    Task UpdateAsync(Tenant tenant, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

/// <summary>
/// Repositorio de certificados.
/// </summary>
public interface ICertificateRepository
{
    Task<Certificate?> GetActiveAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<Certificate>> GetExpiringSoonAsync(int daysThreshold = 30, CancellationToken ct = default);
    Task AddAsync(Certificate certificate, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

/// <summary>
/// Repositorio de webhooks.
/// </summary>
public interface IWebhookRepository
{
    Task<IReadOnlyList<WebhookSubscription>> GetActiveByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<WebhookDelivery>> GetPendingRetriesAsync(int maxCount = 50, CancellationToken ct = default);
    Task AddDeliveryAsync(WebhookDelivery delivery, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
