using DgiiSaas.Application.Interfaces;
using DgiiSaas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DgiiSaas.Infrastructure.Persistence;

public class DgiiDbContextFactory : IDesignTimeDbContextFactory<DgiiDbContext>
{
    public DgiiDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DgiiDbContext>();
        optionsBuilder.UseSqlServer("Server=DESKTOP-N5P1VM2\\SQLEXPRESS;Database=DgiiSaasDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;",
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
