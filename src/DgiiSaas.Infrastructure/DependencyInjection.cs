using DgiiSaas.Application.Interfaces;
using DgiiSaas.Infrastructure.Persistence;
using DgiiSaas.Infrastructure.Repositories;
using DgiiSaas.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DgiiSaas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Interceptor for Auditing entities (CreatedBy, UpdatedBy...) - Can be added later

        services.AddDbContext<DgiiDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("DgiiSaas.Api") // Migrations ran from Api project
            )
        );

        // Repositories
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ICertificateRepository, CertificateRepository>();
        services.AddScoped<IWebhookRepository, WebhookRepository>();

        // Application Services
        services.AddScoped<IDgiiAuthService, DgiiIntegrationService>();
        services.AddScoped<IDgiiSubmissionService, DgiiIntegrationService>();
        services.AddScoped<IXmlGeneratorService, XmlGeneratorService>();
        services.AddScoped<IDigitalSignatureService, DigitalSignatureService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}
