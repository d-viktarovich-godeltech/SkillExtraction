namespace SkillExtraction.Core.Interfaces;

public interface IFileStorageService
{
    Task<(string storagePath, long fileSize)> SaveCvFileAsync(Stream fileStream, string fileName, int userId);
    Task<bool> DeleteCvFileAsync(string storagePath);
    string GetFullPath(string storagePath);
}
