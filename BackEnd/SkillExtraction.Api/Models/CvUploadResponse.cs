namespace SkillExtraction.Api.Models;

public class CvUploadResponse
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public long FileSize { get; set; }
    public List<string> ExtractedSkills { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}
