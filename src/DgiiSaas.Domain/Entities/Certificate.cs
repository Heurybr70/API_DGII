namespace DgiiSaas.Domain.Entities;

/// <summary>
/// Certificado digital por tenant para firma de documentos electrónicos.
/// </summary>
public class Certificate : TenantScopedEntity
{
    public string FriendlyName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string Thumbprint { get; set; } = string.Empty;
    public DateTime NotBefore { get; set; }
    public DateTime NotAfter { get; set; }

    /// <summary>Certificado .p12/.pfx cifrado en Base64.</summary>
    public string EncryptedCertificateData { get; set; } = string.Empty;

    /// <summary>Contraseña cifrada del certificado.</summary>
    public string EncryptedPassword { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public bool IsExpired => DateTime.UtcNow > NotAfter;
    public bool IsExpiringSoon => DateTime.UtcNow.AddDays(30) > NotAfter;
}
