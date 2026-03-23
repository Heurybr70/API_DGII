using Hazelcast;
using Hazelcast.DistributedObjects;
using DgiiSaas.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DgiiSaas.Infrastructure.Hazelcast.Services;

public class HazelcastCacheService : ICacheService
{
    private readonly IHazelcastClient _client;
    private readonly ILogger<HazelcastCacheService> _logger;

    public HazelcastCacheService(IHazelcastClient client, ILogger<HazelcastCacheService> logger)
    {
        _client = client;
        _logger = logger;
    }

    private async Task<IHMap<string, string>> GetMapAsync()
    {
        return await _client.GetMapAsync<string, string>("dgii-saas-cache");
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var map = await GetMapAsync();
            var json = await map.GetAsync(key);
            if (string.IsNullOrEmpty(json))
                return default;

            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo llave {Key} desde Hazelcast.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        try
        {
            var map = await GetMapAsync();
            var json = JsonSerializer.Serialize(value);
            
            if (expiry.HasValue)
            {
                await map.PutAsync(key, json, expiry.Value);
            }
            else
            {
                await map.PutAsync(key, json);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando llave {Key} en Hazelcast.", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            var map = await GetMapAsync();
            await map.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando llave {Key} desde Hazelcast.", key);
        }
    }
}
