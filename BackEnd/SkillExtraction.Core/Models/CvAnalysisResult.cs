namespace SkillExtraction.Core.Models;

public class CvAnalysisResult
{
    public string Summary { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = new();
}
