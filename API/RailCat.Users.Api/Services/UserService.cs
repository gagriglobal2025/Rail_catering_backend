using RailCat.Users.Api.Helpers;
using RailCat.Users.Api.Models.DTOs.User;
using RailCat.Users.Api.Models.Entities;
using RailCat.Users.Api.Repositories.Interfaces;
using RailCat.Users.Api.Services.Interfaces;

namespace RailCat.Users.Api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<StaffUserResponseDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToResponseDto);
    }

    public async Task<StaffUserResponseDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user == null ? null : MapToResponseDto(user);
    }

    public async Task<StaffUserResponseDto> CreateUserAsync(CreateStaffUserDto request)
    {
        var user = new StaffUser
        {
            EmployeeId = request.EmployeeId,
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            StationId = string.IsNullOrWhiteSpace(request.StationId) ? "SC" : request.StationId.Trim(),
            SystemRole = request.SystemRole,
            AccountStatus = "ACTIVE",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Credential = new StaffCredential
            {
                PasswordHash = PasswordHelper.HashPassword(request.Password)
            }
        };

        var created = await _userRepository.AddAsync(user);
        return MapToResponseDto(created);
    }

    public async Task<StaffUserResponseDto?> UpdateUserAsync(Guid id, UpdateStaffUserDto request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        if (!string.IsNullOrWhiteSpace(request.StationId)) user.StationId = request.StationId.Trim();
        user.SystemRole = request.SystemRole;
        user.AccountStatus = request.AccountStatus;

        await _userRepository.UpdateAsync(user);
        return MapToResponseDto(user);
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return false;

        await _userRepository.DeleteAsync(user);
        return true;
    }

    private static StaffUserResponseDto MapToResponseDto(StaffUser user) => new()
    {
        UserId = user.UserId,
        EmployeeId = user.EmployeeId,
        FullName = user.FullName,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        StationId = user.StationId,
        SystemRole = user.SystemRole,
        AccountStatus = user.AccountStatus,
        LastActiveAt = user.LastActiveAt,
        CreatedAt = user.CreatedAt
    };
}
