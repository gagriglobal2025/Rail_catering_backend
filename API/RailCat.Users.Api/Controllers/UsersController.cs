using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailCat.Users.Api.Models.DTOs.Auth;
using RailCat.Users.Api.Models.DTOs.User;
using RailCat.Users.Api.Services.Interfaces;

namespace RailCat.Users.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;

    public UsersController(IUserService userService, IAuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(IEnumerable<StaffUserResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StaffUserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound(new { message = $"Staff member with ID {id} not found." });
        return Ok(user);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(StaffUserResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateStaffUserDto request)
    {
        var created = await _userService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.UserId }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(StaffUserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStaffUserDto request)
    {
        var updated = await _userService.UpdateUserAsync(id, request);
        if (updated == null) return NotFound(new { message = $"Staff member with ID {id} not found." });
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _userService.DeleteUserAsync(id);
        if (!success) return NotFound(new { message = $"Staff member with ID {id} not found." });
        return NoContent();
    }

    /// <summary>
    /// Admin-driven password reset for a staff user (e.g. Cashier forgot POS password)
    /// </summary>
    [HttpPost("{id:guid}/reset-password")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] AdminResetPasswordDto request)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
        {
            return BadRequest(new { message = "New password must be at least 6 characters long." });
        }

        await _authService.ResetPasswordByAdminAsync(id, request.NewPassword);
        return Ok(new { message = "Password reset successfully by Admin." });
    }
}
