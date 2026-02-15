using MediatR;
using SkillExtraction.Core.Models;

namespace SkillExtraction.Core.Queries;

public class GetCvHistoryQuery : IRequest<GetCvHistoryResult>
{
    public int UserId { get; set; }
    public int Limit { get; set; } = 10;
}

public class GetCvHistoryResult
{
    public List<CvUpload> CvUploads { get; set; } = new();
}
