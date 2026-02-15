using MediatR;
using SkillExtraction.Core.Interfaces;
using SkillExtraction.Core.Queries;

namespace SkillExtraction.Core.Handlers;

public class GetCvDetailsHandler : IRequestHandler<GetCvDetailsQuery, GetCvDetailsResult>
{
    private readonly ICvRepository _cvRepository;

    public GetCvDetailsHandler(ICvRepository cvRepository)
    {
        _cvRepository = cvRepository;
    }

    public async Task<GetCvDetailsResult> Handle(GetCvDetailsQuery request, CancellationToken cancellationToken)
    {
        var cv = await _cvRepository.GetCvByIdAsync(request.CvId, request.UserId);

        return new GetCvDetailsResult
        {
            CvUpload = cv
        };
    }
}
