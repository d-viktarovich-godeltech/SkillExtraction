using SkillExtraction.Core.Models;

namespace SkillExtraction.Core.Interfaces;

public interface ICvRepository
{
    Task<CvUpload> CreateCvUploadAsync(CvUpload cvUpload);
    Task<CvUpload?> GetCvByIdAsync(int id, int userId);
    Task<List<CvUpload>> GetUserCvHistoryAsync(int userId, int limit = 10);
    Task<bool> DeleteCvAsync(int id, int userId);
}
