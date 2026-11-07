namespace RailCat.Users.Api.Models.DTOs.User;

public class CreateStaffUserDto
{
    public string EmployeeId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string SystemRole { get; set; } = "COUNTER_OPERATOR";
    public string Password { get; set; } = string.Empty;
}

public class UpdateStaffUserDto
{
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string SystemRole { get; set; } = "COUNTER_OPERATOR";
    public string AccountStatus { get; set; } = "ACTIVE";
}

public class StaffUserResponseDto
{
    public Guid UserId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string SystemRole { get; set; } = string.Empty;
    public string AccountStatus { get; set; } = string.Empty;
    public DateTime? LastActiveAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
