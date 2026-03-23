using DgiiSaas.Domain.Enums;

namespace DgiiSaas.Domain.Entities;

/// <summary>
/// Empresa cliente multi-tenant.
/// </summary>
public class Tenant : BaseEntity
{
    public string Rnc { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public string? Direccion { get; set; }
    public string? Municipio { get; set; }
    public string? Provincia { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? WebSite { get; set; }
    public string? ActividadEconomica { get; set; }
    public bool IsActive { get; set; } = true;
    public OperationEnvironment Environment { get; set; } = OperationEnvironment.Sandbox;

    // Navigation
    public TenantSettings? Settings { get; set; }
    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
    public ICollection<ApiClient> ApiClients { get; set; } = new List<ApiClient>();
    public ICollection<ElectronicDocument> Documents { get; set; } = new List<ElectronicDocument>();
    public ICollection<WebhookSubscription> WebhookSubscriptions { get; set; } = new List<WebhookSubscription>();
}
