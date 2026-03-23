using System.IO;
using DgiiSaas.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace DgiiSaas.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(ILogger<LocalFileStorageService> logger)
    {
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
        _logger = logger;
    }

    public async Task<string> SaveFileAsync(Guid tenantId, string fileName, byte[] content, CancellationToken ct = default)
    {
        var tenantFolder = Path.Combine(_basePath, tenantId.ToString());
        if (!Directory.Exists(tenantFolder))
            Directory.CreateDirectory(tenantFolder);

        // Format is: Storage/{tenantId}/{year}/{month}/fileName
        var today = DateTime.UtcNow;
        var destinationFolder = Path.Combine(tenantFolder, today.Year.ToString(), today.Month.ToString("D2"));
        if (!Directory.Exists(destinationFolder))
            Directory.CreateDirectory(destinationFolder);

        var fullPath = Path.Combine(destinationFolder, fileName);
        await File.WriteAllBytesAsync(fullPath, content, ct);
        
        _logger.LogInformation("Archivo guardado: {Path}", fullPath);
        return fullPath;
    }

    public async Task<byte[]?> GetFileAsync(string filePath, CancellationToken ct = default)
    {
        if (!File.Exists(filePath))
            return null;

        return await File.ReadAllBytesAsync(filePath, ct);
    }
}
