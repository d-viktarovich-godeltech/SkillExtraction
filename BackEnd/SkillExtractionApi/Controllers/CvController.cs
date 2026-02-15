using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillExtractionApi.Data;
using SkillExtractionApi.DTOs;
using SkillExtractionApi.Models;
using SkillExtractionApi.Services;
using System.Security.Claims;
using System.Text.Json;

namespace SkillExtractionApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CvController : ControllerBase
{
    private readonly DuckDbContext _dbContext;
    private readonly FileStorageService _fileStorage;
    private readonly CvProcessingService _cvProcessing;

    public CvController(
        DuckDbContext dbContext,
        FileStorageService fileStorage,
        CvProcessingService cvProcessing)
    {
        _dbContext = dbContext;
        _fileStorage = fileStorage;
        _cvProcessing = cvProcessing;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    [HttpPost("upload")]
    public async Task<ActionResult<CvUploadResponse>> UploadCv([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded" });
        }

        // Validate file extension
        if (!FileStorageService.IsValidFileExtension(file.FileName))
        {
            return BadRequest(new { message = "Invalid file format. Only PDF, PNG, JPG, and JPEG are supported." });
        }

        var userId = GetCurrentUserId();

        try
        {
            // Save file to storage
            string filePath;
            using (var stream = file.OpenReadStream())
            {
                filePath = await _fileStorage.SaveCvFileAsync(stream, file.FileName, userId);
            }

            // Create initial database record
            var cvUpload = new CvUpload
            {
                UserId = userId,
                FileName = file.FileName,
                FilePath = filePath,
                FileSize = file.Length,
                ProcessingStatus = "Processing"
            };

            cvUpload = await _dbContext.CreateCvUploadAsync(cvUpload);

            // Process CV with OpenAI (this might take a while)
            try
            {
                var analysisResult = await _cvProcessing.AnalyzeCvAsync(filePath);

                // Update record with results
                cvUpload.ExtractedSkills = JsonSerializer.Serialize(analysisResult.Skills);
                cvUpload.OpenAiResponse = analysisResult.RawResponse;
                cvUpload.ProcessingStatus = "Completed";
                
                await _dbContext.UpdateCvUploadAsync(cvUpload);

                return Ok(new CvUploadResponse
                {
                    Id = cvUpload.Id,
                    FileName = cvUpload.FileName,
                    UploadDate = cvUpload.UploadDate,
                    FileSize = cvUpload.FileSize,
                    ExtractedSkills = analysisResult.Skills,
                    Summary = analysisResult.Summary,
                    ProcessingStatus = cvUpload.ProcessingStatus
                });
            }
            catch (Exception ex)
            {
                // Update status to failed
                cvUpload.ProcessingStatus = "Failed";
                cvUpload.OpenAiResponse = $"Error: {ex.Message}";
                await _dbContext.UpdateCvUploadAsync(cvUpload);

                return StatusCode(500, new 
                { 
                    message = "CV saved but processing failed", 
                    error = ex.Message,
                    cvId = cvUpload.Id
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to upload CV", error = ex.Message });
        }
    }

    [HttpGet("history")]
    public async Task<ActionResult<CvHistoryResponse>> GetHistory()
    {
        var userId = GetCurrentUserId();
        var uploads = await _dbContext.GetUserCvUploadsAsync(userId);

        var response = new CvHistoryResponse
        {
            Uploads = uploads.Select(cv => new CvUploadResponse
            {
                Id = cv.Id,
                FileName = cv.FileName,
                UploadDate = cv.UploadDate,
                FileSize = cv.FileSize,
                ExtractedSkills = string.IsNullOrEmpty(cv.ExtractedSkills) 
                    ? new List<string>() 
                    : JsonSerializer.Deserialize<List<string>>(cv.ExtractedSkills) ?? new List<string>(),
                Summary = ExtractSummaryFromResponse(cv.OpenAiResponse),
                ProcessingStatus = cv.ProcessingStatus
            }).ToList()
        };

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CvUploadResponse>> GetCvDetails(int id)
    {
        var userId = GetCurrentUserId();
        var cv = await _dbContext.GetCvUploadByIdAsync(id, userId);

        if (cv == null)
        {
            return NotFound(new { message = "CV not found" });
        }

        return Ok(new CvUploadResponse
        {
            Id = cv.Id,
            FileName = cv.FileName,
            UploadDate = cv.UploadDate,
            FileSize = cv.FileSize,
            ExtractedSkills = string.IsNullOrEmpty(cv.ExtractedSkills) 
                ? new List<string>() 
                : JsonSerializer.Deserialize<List<string>>(cv.ExtractedSkills) ?? new List<string>(),
            Summary = ExtractSummaryFromResponse(cv.OpenAiResponse),
            ProcessingStatus = cv.ProcessingStatus
        });
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadCv(int id)
    {
        var userId = GetCurrentUserId();
        var cv = await _dbContext.GetCvUploadByIdAsync(id, userId);

        if (cv == null)
        {
            return NotFound(new { message = "CV not found" });
        }

        try
        {
            var (stream, contentType) = await _fileStorage.GetCvFileAsync(cv.FilePath);
            return File(stream, contentType, cv.FileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { message = "CV file not found on disk" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCv(int id)
    {
        var userId = GetCurrentUserId();
        var cv = await _dbContext.GetCvUploadByIdAsync(id, userId);

        if (cv == null)
        {
            return NotFound(new { message = "CV not found" });
        }

        // Delete from filesystem
        _fileStorage.DeleteCvFile(cv.FilePath);

        // Delete from database
        await _dbContext.DeleteCvUploadAsync(id, userId);

        return NoContent();
    }

    private static string ExtractSummaryFromResponse(string openAiResponse)
    {
        if (string.IsNullOrEmpty(openAiResponse))
            return string.Empty;

        try
        {
            var cleanedResponse = openAiResponse.Trim();
            if (cleanedResponse.StartsWith("```json"))
            {
                cleanedResponse = cleanedResponse.Substring(7);
            }
            if (cleanedResponse.StartsWith("```"))
            {
                cleanedResponse = cleanedResponse.Substring(3);
            }
            if (cleanedResponse.EndsWith("```"))
            {
                cleanedResponse = cleanedResponse.Substring(0, cleanedResponse.Length - 3);
            }
            cleanedResponse = cleanedResponse.Trim();

            var result = JsonSerializer.Deserialize<CvAnalysisResult>(cleanedResponse);
            return result?.Summary ?? string.Empty;
        }
        catch
        {
            return openAiResponse;
        }
    }
}
