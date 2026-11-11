namespace RailCat.Users.Api.Models.DTOs.Auth;

public class LoginRequestDto
{
    /// <summary>
    /// Email (e.g. manager@railcat.local), Employee ID (e.g. STAFF-ADMIN), or Mobile Number (e.g. +919876543210)
    /// </summary>
    public string UserLogin { get; set; } = string.Empty;

    public string EmployeeIdOrEmail
    {
        get => UserLogin;
        set => UserLogin = !string.IsNullOrEmpty(value) ? value : UserLogin;
    }

    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public Guid UserId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
    public string StationId { get; set; } = "SC";
}

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }
}

public class RevokeTokenRequestDto
{
    public string? RefreshToken { get; set; }
}

public class ChangePasswordRequestDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class AdminResetPasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
}
