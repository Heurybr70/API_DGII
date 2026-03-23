using DgiiSaas.Application.Interfaces;
using DgiiSaas.Infrastructure.Hazelcast.Services;
using Hazelcast;
using Hazelcast.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DgiiSaas.Infrastructure.Hazelcast;

public static class DependencyInjection
{
    public static IServiceCollection AddHazelcastInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Hazelcast utilizing the configuration directly
        var hazelcastConfig = configuration.GetSection("Hazelcast");
        services.AddHazelcast(hazelcastConfig);

        // Register Hazelcast specific services
        services.AddSingleton<ICacheService, HazelcastCacheService>();
        services.AddSingleton<IDistributedLockService, HazelcastLockService>();

        return services;
    }
}
