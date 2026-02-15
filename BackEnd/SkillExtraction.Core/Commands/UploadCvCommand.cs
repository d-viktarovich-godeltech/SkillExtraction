using MediatR;
using SkillExtraction.Core.Models;

namespace SkillExtraction.Core.Commands;

public class UploadCvCommand : IRequest<UploadCvResult>
{
    public int UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public Stream FileStream { get; set; } = null!;
}

public class UploadCvResult
{
    public CvUpload CvUpload { get; set; } = null!;
}
