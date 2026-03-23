using DgiiSaas.Application.Interfaces;
using DgiiSaas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DgiiSaas.Infrastructure.Persistence;

public class DgiiDbContext : DbContext
{
    private readonly ITenantContext? _tenantContext;

    public DgiiDbContext(DbContextOptions<DgiiDbContext> options, ITenantContext? tenantContext = null)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantSettings> TenantSettings => Set<TenantSettings>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<ApiClient> ApiClients => Set<ApiClient>();
    public DbSet<ElectronicDocument> Documents => Set<ElectronicDocument>();
    public DbSet<DocumentLine> DocumentLines => Set<DocumentLine>();
    public DbSet<PaymentDetail> PaymentDetails => Set<PaymentDetail>();
    public DbSet<DocumentDiscountCharge> DocumentDiscountCharges => Set<DocumentDiscountCharge>();
    public DbSet<DocumentAdditionalTax> DocumentAdditionalTaxes => Set<DocumentAdditionalTax>();
    public DbSet<DocumentStatusHistory> DocumentStatusHistories => Set<DocumentStatusHistory>();
    public DbSet<DgiiSubmission> DgiiSubmissions => Set<DgiiSubmission>();
    public DbSet<WebhookSubscription> WebhookSubscriptions => Set<WebhookSubscription>();
    public DbSet<WebhookDelivery> WebhookDeliveries => Set<WebhookDelivery>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ContingencyRecord> ContingencyRecords => Set<ContingencyRecord>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // === Tenant ===
        modelBuilder.Entity<Tenant>(e =>
        {
            e.ToTable("Tenants");
            e.HasKey(t => t.Id);
            e.HasIndex(t => t.Rnc).IsUnique();
            e.Property(t => t.Rnc).HasMaxLength(11).IsRequired();
            e.Property(t => t.RazonSocial).HasMaxLength(150).IsRequired();
            e.Property(t => t.NombreComercial).HasMaxLength(150);
            e.Property(t => t.Direccion).HasMaxLength(100);
            e.Property(t => t.Email).HasMaxLength(80);
        });

        // === TenantSettings ===
        modelBuilder.Entity<TenantSettings>(e =>
        {
            e.ToTable("TenantSettings");
            e.HasKey(ts => ts.Id);
            e.HasOne(ts => ts.Tenant).WithOne(t => t.Settings).HasForeignKey<TenantSettings>(ts => ts.TenantId);
            e.Property(ts => ts.DgiiAuthUrl).HasMaxLength(500);
            e.Property(ts => ts.DgiiReceptionUrl).HasMaxLength(500);
            e.Property(ts => ts.DgiiStatusUrl).HasMaxLength(500);
            e.Property(ts => ts.DgiiSeedUrl).HasMaxLength(500);
        });

        // === Certificate ===
        modelBuilder.Entity<Certificate>(e =>
        {
            e.ToTable("Certificates");
            e.HasKey(c => c.Id);
            e.HasOne(c => c.Tenant).WithMany(t => t.Certificates).HasForeignKey(c => c.TenantId);
            e.HasIndex(c => new { c.TenantId, c.Thumbprint }).IsUnique();
            e.Property(c => c.Thumbprint).HasMaxLength(100);
            e.Property(c => c.SubjectName).HasMaxLength(250);
        });

        // === ApiClient ===
        modelBuilder.Entity<ApiClient>(e =>
        {
            e.ToTable("ApiClients");
            e.HasKey(a => a.Id);
            e.HasOne(a => a.Tenant).WithMany(t => t.ApiClients).HasForeignKey(a => a.TenantId);
            e.HasIndex(a => a.ApiKeyHash).IsUnique();
            e.Property(a => a.ApiKeyHash).HasMaxLength(128);
            e.Property(a => a.ClientName).HasMaxLength(100);
        });

        // === ElectronicDocument ===
        modelBuilder.Entity<ElectronicDocument>(e =>
        {
            e.ToTable("Documents");
            e.HasKey(d => d.Id);
            e.HasOne(d => d.Tenant).WithMany(t => t.Documents).HasForeignKey(d => d.TenantId);
            e.HasIndex(d => new { d.TenantId, d.ENCF }).IsUnique();
            e.HasIndex(d => new { d.TenantId, d.IdempotencyKey }).IsUnique().HasFilter("[IdempotencyKey] IS NOT NULL");
            e.HasIndex(d => new { d.TenantId, d.Status });
            e.HasIndex(d => d.DgiiTrackId);
            e.Property(d => d.ENCF).HasMaxLength(13).IsRequired();
            e.Property(d => d.IssuerRnc).HasMaxLength(11);
            e.Property(d => d.IssuerBusinessName).HasMaxLength(150);
            e.Property(d => d.BuyerRnc).HasMaxLength(11);
            e.Property(d => d.BuyerBusinessName).HasMaxLength(150);
            e.Property(d => d.TotalAmount).HasPrecision(18, 2);
            e.Property(d => d.TotalITBIS).HasPrecision(18, 2);
            e.Property(d => d.ExemptAmount).HasPrecision(18, 2);
            e.Property(d => d.TotalTaxableAmount).HasPrecision(18, 2);
            e.Property(d => d.IdempotencyKey).HasMaxLength(100);
            e.Property(d => d.ExternalDocumentId).HasMaxLength(100);
            e.Property(d => d.DgiiTrackId).HasMaxLength(100);
            e.Property(d => d.SecurityCode).HasMaxLength(10);

            // Global tenant filter
            e.HasQueryFilter(d => _tenantContext == null || d.TenantId == _tenantContext.TenantId);
        });

        // === DocumentLine ===
        modelBuilder.Entity<DocumentLine>(e =>
        {
            e.ToTable("DocumentLines");
            e.HasKey(l => l.Id);
            e.HasOne(l => l.Document).WithMany(d => d.Lines).HasForeignKey(l => l.DocumentId).OnDelete(DeleteBehavior.Cascade);
            e.Property(l => l.ItemName).HasMaxLength(80);
            e.Property(l => l.UnitPrice).HasPrecision(20, 4);
            e.Property(l => l.Quantity).HasPrecision(18, 2);
            e.Property(l => l.ItemAmount).HasPrecision(18, 2);
            e.Property(l => l.DiscountAmount).HasPrecision(18, 2);
        });

        // === PaymentDetail ===
        modelBuilder.Entity<PaymentDetail>(e =>
        {
            e.ToTable("PaymentDetails");
            e.HasKey(p => p.Id);
            e.HasOne(p => p.Document).WithMany(d => d.PaymentDetails).HasForeignKey(p => p.DocumentId).OnDelete(DeleteBehavior.Cascade);
            e.Property(p => p.Amount).HasPrecision(18, 2);
        });

        // === DocumentDiscountCharge ===
        modelBuilder.Entity<DocumentDiscountCharge>(e =>
        {
            e.ToTable("DocumentDiscountCharges");
            e.HasKey(dc => dc.Id);
            e.HasOne(dc => dc.Document).WithMany(d => d.DiscountsCharges).HasForeignKey(dc => dc.DocumentId).OnDelete(DeleteBehavior.Cascade);
            e.Property(dc => dc.Amount).HasPrecision(18, 2);
        });

        // === DocumentAdditionalTax ===
        modelBuilder.Entity<DocumentAdditionalTax>(e =>
        {
            e.ToTable("DocumentAdditionalTaxes");
            e.HasKey(t => t.Id);
            e.HasOne(t => t.Document).WithMany(d => d.AdditionalTaxes).HasForeignKey(t => t.DocumentId).OnDelete(DeleteBehavior.Cascade);
            e.Property(t => t.TaxType).HasMaxLength(10);
        });

        // === DocumentStatusHistory ===
        modelBuilder.Entity<DocumentStatusHistory>(e =>
        {
            e.ToTable("DocumentStatusHistory");
            e.HasKey(h => h.Id);
            e.HasOne(h => h.Document).WithMany(d => d.StatusHistory).HasForeignKey(h => h.DocumentId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(h => new { h.DocumentId, h.TransitionDate });
        });

        // === DgiiSubmission ===
        modelBuilder.Entity<DgiiSubmission>(e =>
        {
            e.ToTable("DgiiSubmissions");
            e.HasKey(s => s.Id);
            e.HasOne(s => s.Document).WithMany(d => d.DgiiSubmissions).HasForeignKey(s => s.DocumentId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(s => s.TrackId);
        });

        // === WebhookSubscription ===
        modelBuilder.Entity<WebhookSubscription>(e =>
        {
            e.ToTable("WebhookSubscriptions");
            e.HasKey(w => w.Id);
            e.HasOne(w => w.Tenant).WithMany(t => t.WebhookSubscriptions).HasForeignKey(w => w.TenantId);
            e.Property(w => w.Url).HasMaxLength(500).IsRequired();
            e.Property(w => w.Events).HasMaxLength(1000);
        });

        // === WebhookDelivery ===
        modelBuilder.Entity<WebhookDelivery>(e =>
        {
            e.ToTable("WebhookDeliveries");
            e.HasKey(d => d.Id);
            e.HasOne(d => d.Subscription).WithMany(s => s.Deliveries).HasForeignKey(d => d.WebhookSubscriptionId).OnDelete(DeleteBehavior.Cascade);
        });

        // === AuditLog ===
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.ToTable("AuditLogs");
            e.HasKey(a => a.Id);
            e.HasIndex(a => new { a.TenantId, a.CreatedAt });
            e.HasIndex(a => a.CorrelationId);
            e.Property(a => a.Action).HasMaxLength(100);
            e.Property(a => a.EntityType).HasMaxLength(100);
        });

        // === ContingencyRecord ===
        modelBuilder.Entity<ContingencyRecord>(e =>
        {
            e.ToTable("ContingencyRecords");
            e.HasKey(c => c.Id);
            e.HasOne(c => c.Tenant).WithMany().HasForeignKey(c => c.TenantId);
        });

        // === OutboxEvent ===
        modelBuilder.Entity<OutboxEvent>(e =>
        {
            e.ToTable("OutboxEvents");
            e.HasKey(o => o.Id);
            e.HasIndex(o => new { o.IsProcessed, o.CreatedAt });
            e.Property(o => o.EventType).HasMaxLength(200);
        });
    }
}
