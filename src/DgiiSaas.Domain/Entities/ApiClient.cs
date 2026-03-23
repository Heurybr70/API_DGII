namespace DgiiSaas.Domain.Entities;

/// <summary>
/// Cliente API (credenciales de acceso para integración B2B).
/// </summary>
public class ApiClient : TenantScopedEntity
{
    public string ClientName { get; set; } = string.Empty;
    public string ApiKeyHash { get; set; } = string.Empty;
    public string? ClientSecret { get; set; }
    public string Scopes { get; set; } = "documents:write documents:read";
    public bool IsActive { get; set; } = true;
    public DateTime? LastUsedAt { get; set; }
    public string? AllowedIps { get; set; }
    public int? RateLimitPerMinute { get; set; } = 60;
}
