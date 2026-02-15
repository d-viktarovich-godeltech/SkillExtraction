using MediatR;
using SkillExtraction.Core.Interfaces;
using SkillExtraction.Core.Queries;

namespace SkillExtraction.Core.Handlers;

public class GetCvHistoryHandler : IRequestHandler<GetCvHistoryQuery, GetCvHistoryResult>
{
    private readonly ICvRepository _cvRepository;

    public GetCvHistoryHandler(ICvRepository cvRepository)
    {
        _cvRepository = cvRepository;
    }

    public async Task<GetCvHistoryResult> Handle(GetCvHistoryQuery request, CancellationToken cancellationToken)
    {
        var cvUploads = await _cvRepository.GetUserCvHistoryAsync(request.UserId, request.Limit);

        return new GetCvHistoryResult
        {
            CvUploads = cvUploads
        };
    }
}
