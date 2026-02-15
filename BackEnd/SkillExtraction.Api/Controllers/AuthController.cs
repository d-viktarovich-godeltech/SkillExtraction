using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillExtraction.Api.Models;
using SkillExtraction.Core.Commands;
using SkillExtraction.Core.Queries;
using System.Security.Claims;

namespace SkillExtraction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var command = new RegisterUserCommand
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password
            };

            var result = await _mediator.Send(command);

            return Ok(new LoginResponse
            {
                Token = result.Token,
                User = result.User.Adapt<UserResponse>()
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand
        {
            UsernameOrEmail = request.UsernameOrEmail,
            Password = request.Password
        };

        var result = await _mediator.Send(command);
        
        if (!result.Success || result.User == null)
        {
            return Unauthorized(new { message = result.ErrorMessage ?? "Invalid username/email or password" });
        }

        return Ok(new LoginResponse
        {
            Token = result.Token,
            User = result.User.Adapt<UserResponse>()
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var query = new GetUserProfileQuery { UserId = userId };
        var result = await _mediator.Send(query);
        
        if (result.User == null)
        {
            return NotFound();
        }

        return Ok(result.User.Adapt<UserResponse>());
    }
}

