using DgiiSaas.Application.Interfaces;
using DgiiSaas.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace DgiiSaas.Api.Services;

public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    private Tenant? _currentTenant;

    public TenantContext(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
    }

    public Guid TenantId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return Guid.Empty;

            // 1. Try to get from Items (set by middleware)
            if (httpContext.Items.TryGetValue("TenantId", out var tenantObj) && tenantObj is Guid tenantId)
            {
                return tenantId;
            }

            // 2. Try to get from Claims (if JWT)
            var tenantClaim = httpContext.User.FindFirst("tenant_id");
            if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var claimTenantId))
            {
                return claimTenantId;
            }

            // 3. Fallback to header directly if enabled
            if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var headerTenantId))
            {
                if (Guid.TryParse(headerTenantId.ToString(), out var parsedId))
                {
                     return parsedId;
                }
            }

            return Guid.Empty;
        }
    }

    public Tenant? CurrentTenant => _currentTenant;

    public async Task<Tenant> GetCurrentTenantAsync(CancellationToken ct = default)
    {
        if (_currentTenant != null) return _currentTenant;
        
        var id = TenantId;
        if (id == Guid.Empty) throw new UnauthorizedAccessException("Tenant no identificado.");

        // Using service locator here to avoid circular dependencies with DbContext
        using var scope = _serviceProvider.CreateScope();
        var tenantRepo = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
        
        _currentTenant = await tenantRepo.GetByIdAsync(id, ct);
        if (_currentTenant == null) throw new UnauthorizedAccessException("Tenant no encontrado o inactivo.");

        return _currentTenant;
    }
}
