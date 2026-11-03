using Azure.Core;
using IdentityService.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

/// <summary>
/// Thin controller — only routes requests and returns responses (SRP).
/// Business logic is handled by MediatR handlers.
/// Depends on interfaces, not concrete classes (DIP).
/// Protected with rate limiting policies to prevent brute-force and DoS attacks.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("GeneralPolicy")]
public class IdentityController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<IdentityController> _logger;

    public IdentityController(IMediator mediator, ILogger<IdentityController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // ─── Public Endpoints ─────────────────────────────────────────
    
    /// <summary>Register a new user (public). Rate-limited to prevent automated fake account spam.</summary>
    [AllowAnonymous]
    [EnableRateLimiting("LoginPolicy")]
    [HttpPost("register")]
    public async Task<ActionResult<RegisterUserResponseDto>> Register([FromBody] RegisterUserRequestDto request)
    {
        _logger.LogInformation("Received registration request for {Username}", request.Username);
        var response = await _mediator.Send(request);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    /// <summary>Login for regular users (public). Returns JWT on success. Rate-limited to prevent brute force.</summary>
    [AllowAnonymous]
    [EnableRateLimiting("LoginPolicy")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var response = await _mediator.Send(request);
        return response.Success ? Ok(response) : Unauthorized(response);
    }

    /// <summary>
    /// Admin-only login. Validates credentials AND checks the user's Role (via RoleId FK).
    /// Returns a JWT token with Role = "Admin" claim on success.
    /// Rate-limited to prevent credential stuffing attacks.
    /// </summary>
    [AllowAnonymous]
    [EnableRateLimiting("LoginPolicy")]
    [HttpPost("AdminLogin")]
    public async Task<IActionResult> AdminLogin([FromBody] AdminLoginRequestDto request)
    {
        if (request == null)
            return BadRequest("Invalid request.");

        var response = await _mediator.Send(request);

        if (!response.Success)
            return Unauthorized(response);

        return Ok(response);
    }

    /// <summary>Forgot password (public). Rate-limited to prevent spamming reset requests.</summary>
    [AllowAnonymous]
    [EnableRateLimiting("LoginPolicy")]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }

    // ─── Protected Admin-Only Endpoints (require valid JWT with Role=Admin) ──
    // Example: Add your admin-only management endpoints below, decorated with:
    // [Authorize(Roles = "Admin")]
}
