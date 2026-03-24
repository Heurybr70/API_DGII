using DgiiSaas.Application.Interfaces;
using DgiiSaas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace DgiiSaas.Infrastructure.Persistence;

public class DgiiDbContextFactory : IDesignTimeDbContextFactory<DgiiDbContext>
{
    public DgiiDbContext CreateDbContext(string[] args)
    {
        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .SetBasePath(System.IO.Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<DgiiDbContext>();
        optionsBuilder.UseSqlServer(connectionString,
            b => b.MigrationsAssembly("DgiiSaas.Infrastructure"));

        return new DgiiDbContext(optionsBuilder.Options, new DesignTimeTenantContext());
    }
}

public class DesignTimeTenantContext : ITenantContext
{
    public Guid TenantId => Guid.Empty;

    public Tenant? CurrentTenant => null;

    public Task<Tenant> GetCurrentTenantAsync(CancellationToken ct = default)
    {
        return Task.FromResult(new Tenant());
    }
}
