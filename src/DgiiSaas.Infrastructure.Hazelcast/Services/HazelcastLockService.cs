using Hazelcast;
using Hazelcast.DistributedObjects;
using DgiiSaas.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace DgiiSaas.Infrastructure.Hazelcast.Services;

public class HazelcastLockService : IDistributedLockService
{
    private readonly IHazelcastClient _client;
    private readonly ILogger<HazelcastLockService> _logger;

    public HazelcastLockService(IHazelcastClient client, ILogger<HazelcastLockService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<IAsyncDisposable?> AcquireLockAsync(string resource, TimeSpan expiry, CancellationToken ct = default)
    {
        try
        {
            var map = await _client.GetMapAsync<string, string>("dgii-saas-locks");
            
            // TryLock on map
            var acquired = await map.TryLockAsync(resource, TimeSpan.FromSeconds(2), expiry);
            
            if (acquired)
            {
                _logger.LogDebug("Lock adquirido: {Resource}", resource);
                return new HazelcastMapLock(map, resource, _logger);
            }

            _logger.LogWarning("Lock denegado (en uso): {Resource}", resource);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción adquiriendo lock: {Resource}", resource);
            return null;
        }
    }

    private class HazelcastMapLock : IAsyncDisposable
    {
        private readonly IHMap<string, string> _map;
        private readonly string _resource;
        private readonly ILogger _logger;
        private bool _disposed;

        public HazelcastMapLock(IHMap<string, string> map, string resource, ILogger logger)
        {
            _map = map;
            _resource = resource;
            _logger = logger;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            
            try
            {
                 await _map.UnlockAsync(_resource);
                 _logger.LogDebug("Lock liberado: {Resource}", _resource);
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error liberando lock: {Resource}", _resource);
            }
            finally
            {
                 _disposed = true;
            }
        }
    }
}
