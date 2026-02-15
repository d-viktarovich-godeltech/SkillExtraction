using MediatR;
using SkillExtraction.Core.Commands;
using SkillExtraction.Core.Interfaces;

namespace SkillExtraction.Core.Handlers;

public class DeleteCvHandler : IRequestHandler<DeleteCvCommand, DeleteCvResult>
{
    private readonly ICvRepository _cvRepository;
    private readonly IFileStorageService _fileStorageService;

    public DeleteCvHandler(ICvRepository cvRepository, IFileStorageService fileStorageService)
    {
        _cvRepository = cvRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<DeleteCvResult> Handle(DeleteCvCommand request, CancellationToken cancellationToken)
    {
        // Get CV to find storage path
        var cv = await _cvRepository.GetCvByIdAsync(request.CvId, request.UserId);
        if (cv == null)
        {
            return new DeleteCvResult
            {
                Success = false,
                ErrorMessage = "CV not found"
            };
        }

        // Delete from database
        var deleted = await _cvRepository.DeleteCvAsync(request.CvId, request.UserId);
        if (!deleted)
        {
            return new DeleteCvResult
            {
                Success = false,
                ErrorMessage = "Failed to delete CV"
            };
        }

        // Delete file from storage
        await _fileStorageService.DeleteCvFileAsync(cv.StoragePath);

        return new DeleteCvResult
        {
            Success = true
        };
    }
}
