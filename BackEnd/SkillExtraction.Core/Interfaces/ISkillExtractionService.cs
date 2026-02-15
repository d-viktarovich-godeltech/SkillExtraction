using SkillExtraction.Core.Models;

namespace SkillExtraction.Core.Interfaces;

public interface ISkillExtractionService
{
    Task<CvAnalysisResult> AnalyzeCvAsync(string filePath);
}
