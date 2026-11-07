using RailCat.Users.Api.Models.DTOs.Auth;
using RailCat.Users.Api.Models.DTOs.User;

namespace RailCat.Users.Api.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
}

public interface IUserService
{
    Task<IEnumerable<StaffUserResponseDto>> GetAllUsersAsync();
    Task<StaffUserResponseDto?> GetUserByIdAsync(Guid id);
    Task<StaffUserResponseDto> CreateUserAsync(CreateStaffUserDto request);
    Task<StaffUserResponseDto?> UpdateUserAsync(Guid id, UpdateStaffUserDto request);
    Task<bool> DeleteUserAsync(Guid id);
}

