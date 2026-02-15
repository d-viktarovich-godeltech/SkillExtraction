using MediatR;
using SkillExtraction.Core.Commands;
using SkillExtraction.Core.Interfaces;
using SkillExtraction.Core.Models;
using System.Text.Json;

namespace SkillExtraction.Core.Handlers;

public class UploadCvHandler : IRequestHandler<UploadCvCommand, UploadCvResult>
{
    private readonly ICvRepository _cvRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ISkillExtractionService _skillExtractionService;

    public UploadCvHandler(
        ICvRepository cvRepository, 
        IFileStorageService fileStorageService,
        ISkillExtractionService skillExtractionService)
    {
        _cvRepository = cvRepository;
        _fileStorageService = fileStorageService;
        _skillExtractionService = skillExtractionService;
    }

    public async Task<UploadCvResult> Handle(UploadCvCommand request, CancellationToken cancellationToken)
    {
        // Save file to storage
        var (storagePath, fileSize) = await _fileStorageService.SaveCvFileAsync(
            request.FileStream, 
            request.FileName, 
            request.UserId);

        // Get full path for analysis
        var fullPath = _fileStorageService.GetFullPath(storagePath);

        // Analyze CV
        var analysisResult = await _skillExtractionService.AnalyzeCvAsync(fullPath);

        // Create CV upload record
        var cvUpload = new CvUpload
        {
            UserId = request.UserId,
            FileName = request.FileName,
            StoragePath = storagePath,
            FileSize = fileSize,
            ExtractedSkills = analysisResult.Skills,
            Summary = analysisResult.Summary,
            AnalysisResult = JsonSerializer.Serialize(analysisResult)
        };

        var savedCv = await _cvRepository.CreateCvUploadAsync(cvUpload);

        return new UploadCvResult
        {
            CvUpload = savedCv
        };
    }
}
