using MediatR;
using SkillExtraction.Core.Models;

namespace SkillExtraction.Core.Queries;

public class GetCvDetailsQuery : IRequest<GetCvDetailsResult>
{
    public int CvId { get; set; }
    public int UserId { get; set; }
}

public class GetCvDetailsResult
{
    public CvUpload? CvUpload { get; set; }
}
