namespace SkillExtractionApi.Services;

public class FileStorageService
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

    public async Task<string> SaveCvFileAsync(Stream fileStream, string originalFileName, int userId)
    {
        // Generate unique filename
        var extension = Path.GetExtension(originalFileName);
        var uniqueFileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(_storagePath, uniqueFileName);

        // Save file
        using var fileStreamWriter = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fileStreamWriter);

        return filePath;
    }

    public async Task<(Stream stream, string contentType)> GetCvFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("CV file not found");
        }

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var contentType = extension switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };

        var memory = new MemoryStream();
        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            await stream.CopyToAsync(memory);
        }
        memory.Position = 0;

        return (memory, contentType);
    }

    public bool DeleteCvFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return true;
        }
        return false;
    }

    public static bool IsValidFileExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension is ".pdf" or ".png" or ".jpg" or ".jpeg";
    }
}
