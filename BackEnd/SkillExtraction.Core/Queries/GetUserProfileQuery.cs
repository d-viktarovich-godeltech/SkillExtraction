using MediatR;
using SkillExtraction.Core.Models;

namespace SkillExtraction.Core.Queries;

public class GetUserProfileQuery : IRequest<GetUserProfileResult>
{
    public int UserId { get; set; }
}

public class GetUserProfileResult
{
    public User? User { get; set; }
}
