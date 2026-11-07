namespace RailCat.Users.Api.Models.Entities;

public class StaffUser
{
    public Guid UserId { get; set; } = Guid.NewGuid();
    public string EmployeeId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string SystemRole { get; set; } = "COUNTER_OPERATOR"; // ADMIN, COUNTER_OPERATOR, KITCHEN_STAFF
    public string AccountStatus { get; set; } = "ACTIVE"; // ACTIVE, INACTIVE
    public DateTime? LastActiveAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public StaffCredential? Credential { get; set; }
    public ICollection<StaffSession> Sessions { get; set; } = new List<StaffSession>();
}

