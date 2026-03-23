using DgiiSaas.Domain.Enums;

namespace DgiiSaas.Domain.Entities;

/// <summary>
/// Configuración DGII por tenant.
/// </summary>
public class TenantSettings : BaseEntity
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    // URLs DGII
    public string DgiiAuthUrl { get; set; } = string.Empty;
    public string DgiiReceptionUrl { get; set; } = string.Empty;
    public string DgiiStatusUrl { get; set; } = string.Empty;
    public string DgiiSeedUrl { get; set; } = string.Empty;

    // Credenciales DGII (encrypted)
    public string? DgiiUsername { get; set; }
    public string? DgiiPasswordEncrypted { get; set; }

    // Tipos de e-CF habilitados
    public string EnabledDocumentTypes { get; set; } = "31,32,33,34";

    // Webhook configuration
    public int WebhookMaxRetries { get; set; } = 5;
    public int WebhookRetryIntervalSeconds { get; set; } = 60;

    // Contingency
    public bool IsInContingency { get; set; }
    public string? ContingencyReason { get; set; }
    public DateTime? ContingencyStartedAt { get; set; }
}
