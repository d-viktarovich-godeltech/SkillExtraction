using MediatR;

namespace SkillExtraction.Core.Commands;

public class DeleteCvCommand : IRequest<DeleteCvResult>
{
    public int CvId { get; set; }
    public int UserId { get; set; }
}

public class DeleteCvResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
