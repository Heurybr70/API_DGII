namespace DgiiSaas.Domain.Entities;

/// <summary>
/// Suscripción de webhook por tenant.
/// </summary>
public class WebhookSubscription : TenantScopedEntity
{
    public string Url { get; set; } = string.Empty;
    public string Events { get; set; } = string.Empty; // comma-separated: "document.accepted,document.rejected"
    public string? Secret { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }

    public ICollection<WebhookDelivery> Deliveries { get; set; } = new List<WebhookDelivery>();
}

/// <summary>
/// Registro de entrega de webhook.
/// </summary>
public class WebhookDelivery : BaseEntity
{
    public Guid WebhookSubscriptionId { get; set; }
    public WebhookSubscription Subscription { get; set; } = null!;

    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public int HttpStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public bool IsSuccess { get; set; }
    public int AttemptNumber { get; set; } = 1;
    public DateTime? NextRetryAt { get; set; }
    public DateTime DeliveredAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Log de auditoría inmutable.
/// </summary>
public class AuditLog : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? CorrelationId { get; set; }
}

/// <summary>
/// Registro de contingencia.
/// </summary>
public class ContingencyRecord : TenantScopedEntity
{
    public string Reason { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int DocumentsAffected { get; set; }
    public bool IsResolved { get; set; }
    public string? ResolutionNotes { get; set; }
}

/// <summary>
/// Evento outbox para garantizar consistencia.
/// </summary>
public class OutboxEvent : BaseEntity
{
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
    public string? Error { get; set; }
}
