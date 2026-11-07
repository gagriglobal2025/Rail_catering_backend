namespace RailCat.Catalog.Api.Models.Entities;

public class MenuCategory
{
    public string CategoryId { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>();
}

public class MenuItem
{
    public string ItemId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string EmojiIcon { get; set; } = "🍲";
    public string? ImageUrl { get; set; }
    public bool IsVegetarian { get; set; } = true;
    public bool IsAvailable { get; set; } = true;
    public bool IsSoldOut { get; set; }
    public bool IsBestseller { get; set; }
    public bool IsSpicy { get; set; }
    public bool IsKidsChoice { get; set; }
    public decimal? Rating { get; set; } = 4.5m;
    public int RatingCount { get; set; }
    public string? PrepTime { get; set; } = "10-12 mins";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public MenuCategory? Category { get; set; }
}

public class ConcessionPolicy
{
    public string PolicyId { get; set; } = string.Empty;
    public string PolicyName { get; set; } = string.Empty;
    public string CustomerType { get; set; } = "RAILWAY_EMPLOYEE";
    public decimal DiscountPercentage { get; set; } = 20.00m;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class RailwayConcessionEmployee
{
    public string EmployeeId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string ShiftPolicy { get; set; } = "Standard";
    public string VerificationStatus { get; set; } = "PENDING"; // ACTIVE, PENDING, REJECTED
    public string? DeviceName { get; set; }
    public string? DeviceToken { get; set; }
    public bool DeviceLocked { get; set; }
    public string? IdCardUrl { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAt { get; set; }
}

