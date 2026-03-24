using DgiiSaas.Api.Middleware;
using DgiiSaas.Api.Services;
using DgiiSaas.Application;
using DgiiSaas.Application.Interfaces;
using DgiiSaas.Infrastructure;
using DgiiSaas.Infrastructure.Hazelcast;
using DgiiSaas.Infrastructure.Persistence;
using DgiiSaas.Infrastructure.Services;
using Hangfire;
using Scalar.AspNetCore;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog Setup
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
                 .Enrich.FromLogContext()
                 .WriteTo.Console());

// 2. Add Layer Dependencies
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
// builder.Services.AddHazelcastInfrastructure(builder.Configuration);
builder.Services.AddSingleton<ICacheService, DummyCacheService>();
builder.Services.AddSingleton<IDistributedLockService, DummyLockService>();

// 3. API specific services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddHttpClient();
builder.Services.AddHealthChecks();

// 4. Hangfire configuration
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();
builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.AddTransient<WebhookDeliveryJob>();

builder.Services.AddEndpointsApiExplorer();
// Swagger / OpenAPI Removed due to .NET 10 Source Generator conflicts with Swashbuckle
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.MapOpenApi();
app.MapScalarApiReference(options => 
{
    options.WithTitle("API DGII SaaS")
           .WithTheme(ScalarTheme.Moon);
});

if (app.Environment.IsDevelopment())
{
    // development-specific settings
}

app.UseRouting();

// Add Multitenant API Key Auth Middleware
app.UseMiddleware<ApiKeyAuthMiddleware>();

app.UseAuthorization();

// Hangfire Dashboard (in real world restrict with AuthorizationFilter)
app.UseHangfireDashboard("/hangfire");

app.MapControllers();

app.MapHealthChecks("/health");
app.MapHealthChecks("/healthy");

// Redirects for convenience
app.MapGet("/", () => Results.Redirect("/scalar/v1"));
app.MapGet("/swagger", () => Results.Redirect("/scalar/v1"));

app.Run();

public class DummyCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) => Task.FromResult(default(T));
    public Task RemoveAsync(string key, CancellationToken ct = default) => Task.CompletedTask;
    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default) => Task.CompletedTask;
}

public class DummyLockService : IDistributedLockService
{
    public Task<IAsyncDisposable?> AcquireLockAsync(string resource, TimeSpan expiry, CancellationToken ct = default)
    {
        IAsyncDisposable? dummy = new DummyDisposable();
        return Task.FromResult(dummy);
    }
    
    private class DummyDisposable : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
