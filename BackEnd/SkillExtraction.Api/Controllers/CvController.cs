using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillExtraction.Api.DTOs;
using SkillExtraction.Core.Commands;
using SkillExtraction.Core.Queries;
using System.Security.Claims;

namespace SkillExtraction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CvController : ControllerBase
{
    private readonly IMediator _mediator;

    public CvController(IMediator mediator)
    {
        _mediator = mediator;
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
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not (".pdf" or ".png" or ".jpg" or ".jpeg"))
        {
            return BadRequest(new { message = "Invalid file format. Only PDF, PNG, JPG, and JPEG are supported." });
        }

        var userId = GetCurrentUserId();

        try
        {
            using var stream = file.OpenReadStream();
            var command = new UploadCvCommand
            {
                UserId = userId,
                FileName = file.FileName,
                FileStream = stream
            };

            var result = await _mediator.Send(command);

            return Ok(result.CvUpload.Adapt<CvUploadResponse>());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to upload and process CV", error = ex.Message });
        }
    }

    [HttpGet("history")]
    public async Task<ActionResult<CvHistoryResponse>> GetHistory()
    {
        var userId = GetCurrentUserId();
        
        var query = new GetCvHistoryQuery { UserId = userId };
        var result = await _mediator.Send(query);

        var response = new CvHistoryResponse
        {
            Uploads = result.CvUploads.Adapt<List<CvUploadResponse>>()
        };

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CvUploadResponse>> GetCvDetails(int id)
    {
        var userId = GetCurrentUserId();
        
        var query = new GetCvDetailsQuery { CvId = id, UserId = userId };
        var result = await _mediator.Send(query);

        if (result.CvUpload == null)
        {
            return NotFound(new { message = "CV not found" });
        }

        return Ok(result.CvUpload.Adapt<CvUploadResponse>());
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCv(int id)
    {
        var userId = GetCurrentUserId();

        var command = new DeleteCvCommand { CvId = id, UserId = userId };
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage ?? "CV not found" });
        }

        return NoContent();
    }
}

