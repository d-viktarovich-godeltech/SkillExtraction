using Microsoft.Extensions.Configuration;
using SkillExtraction.Core.Interfaces;

namespace SkillExtraction.Data.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _storagePath;

    public FileStorageService(IConfiguration configuration)
    {
        _storagePath = configuration["FileStorage:CvStoragePath"] 
            ?? throw new ArgumentNullException("CV storage path not configured");

        // Ensure directory exists
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<(string storagePath, long fileSize)> SaveCvFileAsync(Stream fileStream, string fileName, int userId)
    {
        // Generate unique filename
        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(_storagePath, uniqueFileName);

        // Save file
        using var fileStreamWriter = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fileStreamWriter);

        var fileInfo = new FileInfo(filePath);
        // Return only the filename, not the full path
        return (uniqueFileName, fileInfo.Length);
    }

    public Task<bool> DeleteCvFileAsync(string storagePath)
    {
        if (File.Exists(storagePath))
        {
            File.Delete(storagePath);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public string GetFullPath(string storagePath)
    {
        // If the path is already absolute, return it
        if (Path.IsPathRooted(storagePath))
        {
            return storagePath;
        }
        
        // Otherwise, combine with base storage path
        return Path.Combine(_storagePath, storagePath);
    }

    public static bool IsValidFileExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension is ".pdf" or ".png" or ".jpg" or ".jpeg";
    }
}
