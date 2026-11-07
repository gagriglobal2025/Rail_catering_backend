namespace RailCat.Users.Api.Models.Entities;

public class StaffCredential
{
    public Guid UserId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public int FailedLogins { get; set; }
    public DateTime? LockedUntil { get; set; }
    public DateTime PasswordUpdatedAt { get; set; } = DateTime.UtcNow;

    public StaffUser? User { get; set; }
}
