namespace DgiiSaas.Domain.Entities;

/// <summary>
/// Entidad base con campos comunes para auditoría y multi-tenancy.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Entidad base con TenantId obligatorio.
/// </summary>
public abstract class TenantScopedEntity : BaseEntity
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
}
