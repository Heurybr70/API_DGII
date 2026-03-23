using DgiiSaas.Application.Interfaces;
using DgiiSaas.Domain.Entities;
using DgiiSaas.Domain.Enums;
using DgiiSaas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DgiiSaas.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly DgiiDbContext _db;

    public DocumentRepository(DgiiDbContext db) => _db = db;

    public async Task<ElectronicDocument?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Documents
            .Include(d => d.Lines)
            .Include(d => d.PaymentDetails)
            .Include(d => d.StatusHistory)
            .Include(d => d.DgiiSubmissions)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<ElectronicDocument?> GetByIdempotencyKeyAsync(Guid tenantId, string key, CancellationToken ct = default) =>
        await _db.Documents.IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.IdempotencyKey == key, ct);

    public async Task<ElectronicDocument?> GetByEncfAsync(Guid tenantId, string encf, CancellationToken ct = default) =>
        await _db.Documents.IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.ENCF == encf, ct);

    public async Task<IReadOnlyList<ElectronicDocument>> GetByStatusAsync(Guid tenantId, DocumentStatus status, int maxCount = 50, CancellationToken ct = default) =>
        await _db.Documents.IgnoreQueryFilters()
            .Where(d => d.TenantId == tenantId && d.Status == status)
            .OrderBy(d => d.CreatedAt)
            .Take(maxCount)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ElectronicDocument>> GetPendingSubmissionAsync(int maxCount = 50, CancellationToken ct = default) =>
        await _db.Documents.IgnoreQueryFilters()
            .Where(d => d.Status == DocumentStatus.ReceivedFromClient || d.Status == DocumentStatus.Validated)
            .OrderBy(d => d.CreatedAt)
            .Take(maxCount)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ElectronicDocument>> GetPendingStatusCheckAsync(int maxCount = 50, CancellationToken ct = default) =>
        await _db.Documents.IgnoreQueryFilters()
            .Where(d => d.Status == DocumentStatus.TrackIdReceived || d.Status == DocumentStatus.InProcess)
            .OrderBy(d => d.SubmittedAt)
            .Take(maxCount)
            .ToListAsync(ct);

    public async Task AddAsync(ElectronicDocument document, CancellationToken ct = default) =>
        await _db.Documents.AddAsync(document, ct);

    public Task UpdateAsync(ElectronicDocument document, CancellationToken ct = default)
    {
        _db.Documents.Update(document);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);
}

public class TenantRepository : ITenantRepository
{
    private readonly DgiiDbContext _db;

    public TenantRepository(DgiiDbContext db) => _db = db;

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Tenants.Include(t => t.Settings).FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<Tenant?> GetByRncAsync(string rnc, CancellationToken ct = default) =>
        await _db.Tenants.Include(t => t.Settings).FirstOrDefaultAsync(t => t.Rnc == rnc, ct);

    public async Task<TenantSettings?> GetSettingsAsync(Guid tenantId, CancellationToken ct = default) =>
        await _db.TenantSettings.FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);

    public async Task AddAsync(Tenant tenant, CancellationToken ct = default) =>
        await _db.Tenants.AddAsync(tenant, ct);

    public Task UpdateAsync(Tenant tenant, CancellationToken ct = default)
    {
        _db.Tenants.Update(tenant);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);
}

public class CertificateRepository : ICertificateRepository
{
    private readonly DgiiDbContext _db;

    public CertificateRepository(DgiiDbContext db) => _db = db;

    public async Task<Certificate?> GetActiveAsync(Guid tenantId, CancellationToken ct = default) =>
        await _db.Certificates.IgnoreQueryFilters()
            .Where(c => c.TenantId == tenantId && c.IsActive && c.NotAfter > DateTime.UtcNow)
            .OrderByDescending(c => c.NotAfter)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<Certificate>> GetExpiringSoonAsync(int daysThreshold = 30, CancellationToken ct = default) =>
        await _db.Certificates.IgnoreQueryFilters()
            .Where(c => c.IsActive && c.NotAfter <= DateTime.UtcNow.AddDays(daysThreshold))
            .ToListAsync(ct);

    public async Task AddAsync(Certificate certificate, CancellationToken ct = default) =>
        await _db.Certificates.AddAsync(certificate, ct);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);
}

public class WebhookRepository : IWebhookRepository
{
    private readonly DgiiDbContext _db;

    public WebhookRepository(DgiiDbContext db) => _db = db;

    public async Task<IReadOnlyList<WebhookSubscription>> GetActiveByTenantAsync(Guid tenantId, CancellationToken ct = default) =>
        await _db.WebhookSubscriptions.IgnoreQueryFilters()
            .Where(w => w.TenantId == tenantId && w.IsActive)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<WebhookDelivery>> GetPendingRetriesAsync(int maxCount = 50, CancellationToken ct = default) =>
        await _db.WebhookDeliveries
            .Where(d => !d.IsSuccess && d.NextRetryAt != null && d.NextRetryAt <= DateTime.UtcNow)
            .OrderBy(d => d.NextRetryAt)
            .Take(maxCount)
            .ToListAsync(ct);

    public async Task AddDeliveryAsync(WebhookDelivery delivery, CancellationToken ct = default) =>
        await _db.WebhookDeliveries.AddAsync(delivery, ct);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);
}
