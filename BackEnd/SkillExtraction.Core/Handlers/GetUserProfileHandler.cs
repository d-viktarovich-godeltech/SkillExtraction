using MediatR;
using SkillExtraction.Core.Interfaces;
using SkillExtraction.Core.Queries;

namespace SkillExtraction.Core.Handlers;

public class GetUserProfileHandler : IRequestHandler<GetUserProfileQuery, GetUserProfileResult>
{
    private readonly IUserRepository _userRepository;

    public GetUserProfileHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<GetUserProfileResult> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(request.UserId);

        return new GetUserProfileResult
        {
            User = user
        };
    }
}
