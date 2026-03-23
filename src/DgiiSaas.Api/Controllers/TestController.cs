using DgiiSaas.Application.Interfaces;
using DgiiSaas.Domain.Entities;
using DgiiSaas.Domain.Enums;
using DgiiSaas.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DgiiSaas.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TestController : ControllerBase
{
    private readonly DgiiDbContext _dbContext;
    private readonly IDigitalSignatureService _signatureService;

    public TestController(DgiiDbContext dbContext, IDigitalSignatureService signatureService)
    {
        _dbContext = dbContext;
        _signatureService = signatureService;
    }

    [HttpPost("seed")]
    public async Task<IActionResult> SeedDatabase([FromBody] SeedRequest request)
    {
        try 
        {
            var tenantId = Guid.NewGuid();

            // Borrar si ya existe para evitar errores de duplicidad en pruebas
            var existing = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Rnc == "130000000");
            if (existing != null)
            {
                _dbContext.Tenants.Remove(existing);
                await _dbContext.SaveChangesAsync();
            }

            var tenant = new Tenant
            {
                Id = tenantId,
                RazonSocial = "Empresa de Pruebas DGII",
                Rnc = "130000000",
                IsActive = true,
                Environment = OperationEnvironment.Sandbox,
                Settings = new TenantSettings
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    EnabledDocumentTypes = "31,32,33",
                    DgiiAuthUrl = "https://ecf.dgii.gov.do/testecf/autenticacion/api/Autenticacion/Semilla",
                    DgiiReceptionUrl = "https://ecf.dgii.gov.do/testecf/recepcion/api/RecepcionFcpe"
                }
            };

            // For simplicity, request contains the Base64 of the PFX. In a real scenario it's uploaded via IFormFile.
            if (!string.IsNullOrEmpty(request.Base64Pfx) && request.Base64Pfx != "test")
            {
                var cert = new Certificate
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    FriendlyName = "test_cert.pfx",
                    EncryptedCertificateData = request.Base64Pfx, // Should be Encrypted in real life
                    EncryptedPassword = request.PfxPassword ?? string.Empty, // Should be Encrypted in real life
                    NotBefore = DateTime.UtcNow,
                    NotAfter = DateTime.UtcNow.AddYears(1)
                };
                
                _dbContext.Certificates.Add(cert);
            }

            _dbContext.Tenants.Add(tenant);
            
            await _dbContext.SaveChangesAsync();

            return Ok(new { 
                Message = "Tenant creado exitosamente", 
                TenantId = tenantId, 
                ApiKey = "Por implementar (usar X-Tenant-Id header en postman)"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message, Inner = ex.InnerException?.Message, Stack = ex.StackTrace });
        }
    }

    [HttpPost("test-signature")]
    public async Task<IActionResult> TestSignature([FromBody] string xmlToSign, [FromHeader(Name = "X-Tenant-Id")] Guid tenantId)
    {
        try
        {
             var signedXml = await _signatureService.SignXmlAsync(xmlToSign, tenantId);
             
             return Ok(new { SignedXml = signedXml });
        }
        catch (Exception ex)
        {
             return BadRequest(new { Error = ex.Message, StackTrace = ex.StackTrace });
        }
    }
}

public class SeedRequest
{
    public string? Base64Pfx { get; set; }
    public string? PfxPassword { get; set; }
}
