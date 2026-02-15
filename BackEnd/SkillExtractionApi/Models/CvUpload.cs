namespace SkillExtractionApi.Models;

public class CvUpload
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; } = DateTime.UtcNow;
    public long FileSize { get; set; }
    public string ExtractedSkills { get; set; } = string.Empty; // JSON
    public string OpenAiResponse { get; set; } = string.Empty; // JSON
    public string ProcessingStatus { get; set; } = "Pending"; // Pending, Processing, Completed, Failed
}
