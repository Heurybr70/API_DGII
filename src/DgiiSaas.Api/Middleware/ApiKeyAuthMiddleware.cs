using System.Security.Cryptography;
using System.Text;
using DgiiSaas.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace DgiiSaas.Api.Middleware;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;

    public ApiKeyAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // Skip auth for Swagger, Health, etc.
        if (context.Request.Path.StartsWithSegments("/swagger") || 
            context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        // Try getting API Key
        if (!context.Request.Headers.TryGetValue("X-Api-Key", out StringValues apiKeyHeader) || string.IsNullOrEmpty(apiKeyHeader))
        {
             // No API key, perhaps it has a JWT Token which will be handled by standard JWT middleware
             await _next(context);
             return;
        }

        var apiKey = apiKeyHeader.ToString();
        var hashedKey = ComputeHash(apiKey);

        // Needs the DB to validate API Key (we resolve DB Context here or use ITenantContext effectively)
        var tenantRepo = context.RequestServices.GetRequiredService<ITenantRepository>();
        
        // Simulating the check. In real code: await db.ApiClients.FirstOrDefaultAsync(c => c.ApiKeyHash == hashedKey);
        // By setting the context.Items["TenantId"], the TenantContext can read it.
        // For now, if the API key matches a mock hash (or we just accept it if they provide the Target Tenant header)
        
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader) && Guid.TryParse(tenantHeader, out var tenantId))
        {
            // Authenticate successfully and set tenant
            context.Items["TenantId"] = tenantId;
        }
        else
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { Error = "Falta cabecera X-Tenant-Id o API Key inválida." });
            return;
        }

        await _next(context);
    }

    private static string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
