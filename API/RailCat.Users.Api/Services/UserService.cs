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
        var user = await _userRepository.GetByEmailOrEmployeeIdAsync(request.EmailOrEmployeeId);
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
            Role = user.SystemRole
        };
    }
}

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
        SystemRole = user.SystemRole,
        AccountStatus = user.AccountStatus,
        LastActiveAt = user.LastActiveAt,
        CreatedAt = user.CreatedAt
    };
}

