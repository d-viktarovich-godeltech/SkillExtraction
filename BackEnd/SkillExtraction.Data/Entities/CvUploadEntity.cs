namespace SkillExtraction.Data.Entities;

public class CvUploadEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public long FileSize { get; set; }
    public string ExtractedSkills { get; set; } = string.Empty; // JSON array
    public string Summary { get; set; } = string.Empty;
    public string AnalysisResult { get; set; } = string.Empty;
}
