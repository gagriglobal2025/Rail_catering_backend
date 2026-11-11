using RailCat.Users.Api.Models.DTOs.Auth;
using RailCat.Users.Api.Models.DTOs.User;

namespace RailCat.Users.Api.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress = null, string? userAgent = null);
    Task<RefreshTokenResponseDto> RefreshTokenAsync(string refreshToken, string? ipAddress = null, string? userAgent = null);
    Task<bool> RevokeTokenAsync(string refreshToken);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request);
    Task<bool> ResetPasswordByAdminAsync(Guid userId, string newPassword);
    Task<UserProfileResponseDto> GetProfileAsync(Guid userId);
    Task<UserProfileResponseDto> UpdateProfileAsync(Guid userId, UpdateProfileDto request);
    Task<bool> LogoutAsync(Guid userId);
}

public interface IUserService
{
    Task<IEnumerable<StaffUserResponseDto>> GetAllUsersAsync();
    Task<StaffUserResponseDto?> GetUserByIdAsync(Guid id);
    Task<StaffUserResponseDto> CreateUserAsync(CreateStaffUserDto request);
    Task<StaffUserResponseDto?> UpdateUserAsync(Guid id, UpdateStaffUserDto request);
    Task<bool> DeleteUserAsync(Guid id);
}
