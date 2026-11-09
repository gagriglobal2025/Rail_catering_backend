using RailCat.Users.Api.Helpers;
using RailCat.Users.Api.Models.DTOs.Auth;
using RailCat.Users.Api.Models.DTOs.User;
using RailCat.Users.Api.Models.Entities;
using RailCat.Users.Api.Repositories.Interfaces;
using RailCat.Users.Api.Services.Interfaces;

namespace RailCat.Users.Api.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtHelper _jwtHelper;

    public AuthService(IUserRepository userRepository, JwtHelper jwtHelper)
    {
        _userRepository = userRepository;
        _jwtHelper = jwtHelper;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var identifier = request.UserLogin?.Trim() ?? string.Empty;
        var user = await _userRepository.GetByIdentifierAsync(identifier);
        
        if (user == null || user.Credential == null)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (user.AccountStatus != "ACTIVE")
        {
            throw new UnauthorizedAccessException("Account is disabled.");
        }

        if (!PasswordHelper.VerifyPassword(request.Password, user.Credential.PasswordHash))
        {
            user.Credential.FailedLogins++;
            await _userRepository.SaveChangesAsync();
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        user.Credential.FailedLogins = 0;
        user.LastActiveAt = DateTime.UtcNow;
        await _userRepository.SaveChangesAsync();

        var (token, expiresAt) = _jwtHelper.GenerateToken(user);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            UserId = user.UserId,
            EmployeeId = user.EmployeeId,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.SystemRole,
            StationId = user.StationId
        };
    }

    public async Task<UserProfileResponseDto> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("Staff user not found.");
        }

        return new UserProfileResponseDto
        {
            UserId = user.UserId,
            EmployeeId = user.EmployeeId,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            StationId = user.StationId,
            Role = user.SystemRole,
            AccountStatus = user.AccountStatus,
            LastActiveAt = user.LastActiveAt,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserProfileResponseDto> UpdateProfileAsync(Guid userId, UpdateProfileDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("Staff user not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            user.FullName = request.FullName.Trim();
        }

        user.PhoneNumber = request.PhoneNumber?.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        return new UserProfileResponseDto
        {
            UserId = user.UserId,
            EmployeeId = user.EmployeeId,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            StationId = user.StationId,
            Role = user.SystemRole,
            AccountStatus = user.AccountStatus,
            LastActiveAt = user.LastActiveAt,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<bool> LogoutAsync(Guid userId)
    {
        await _userRepository.ClearUserSessionsAsync(userId);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.Credential == null)
        {
            throw new KeyNotFoundException("Staff user not found.");
        }

        if (!PasswordHelper.VerifyPassword(request.CurrentPassword, user.Credential.PasswordHash))
        {
            throw new UnauthorizedAccessException("Current password is incorrect.");
        }

        if (request.NewPassword != request.ConfirmNewPassword)
        {
            throw new ArgumentException("New password and confirmation password do not match.");
        }

        var newHash = PasswordHelper.HashPassword(request.NewPassword);
        await _userRepository.UpdatePasswordAsync(userId, newHash);
        return true;
    }

    public async Task<bool> ResetPasswordByAdminAsync(Guid userId, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.Credential == null)
        {
            throw new KeyNotFoundException("Staff user not found.");
        }

        var newHash = PasswordHelper.HashPassword(newPassword);
        await _userRepository.UpdatePasswordAsync(userId, newHash);
        return true;
    }
}
