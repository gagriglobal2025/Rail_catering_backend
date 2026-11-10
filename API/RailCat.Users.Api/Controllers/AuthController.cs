using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailCat.Users.Api.Models.DTOs.Auth;
using RailCat.Users.Api.Models.DTOs.User;
using RailCat.Users.Api.Services.Interfaces;

namespace RailCat.Users.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Staff Login using Email, Employee ID, or Mobile Number
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Retrieve current authenticated staff user profile
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(new { message = "Invalid auth token." });

        var profile = await _authService.GetProfileAsync(userId.Value);
        return Ok(profile);
    }

    /// <summary>
    /// Self-service profile update (Name, Phone number)
    /// </summary>
    [Authorize]
    [HttpPut("profile")]
    [ProducesResponseType(typeof(UserProfileResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(new { message = "Invalid auth token." });

        var updated = await _authService.UpdateProfileAsync(userId.Value, request);
        return Ok(updated);
    }

    /// <summary>
    /// Self-service Password Change for logged-in staff member
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(new { message = "Invalid auth token." });

        await _authService.ChangePasswordAsync(userId.Value, request);
        return Ok(new { message = "Password changed successfully." });
    }

    /// <summary>
    /// Invalidate active user session on logout
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var userId = GetCurrentUserId();
        if (userId != null)
        {
            await _authService.LogoutAsync(userId.Value);
        }
        return Ok(new { message = "Logged out successfully." });
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }
        return userId;
    }
}
