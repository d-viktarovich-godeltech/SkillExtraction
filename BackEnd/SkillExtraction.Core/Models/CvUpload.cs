namespace SkillExtraction.Core.Models;

public class CvUpload
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; } = DateTime.UtcNow;
    public long FileSize { get; set; }
    public List<string> ExtractedSkills { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public string AnalysisResult { get; set; } = string.Empty;
}
