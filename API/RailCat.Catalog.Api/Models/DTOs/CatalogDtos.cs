namespace RailCat.Catalog.Api.Models.DTOs;

public class CategoryResponseDto
{
    public string CategoryId { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class MenuItemResponseDto
{
    public string ItemId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string EmojiIcon { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsVegetarian { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsSoldOut { get; set; }
    public bool IsBestseller { get; set; }
    public bool IsSpicy { get; set; }
    public bool IsKidsChoice { get; set; }
    public decimal? Rating { get; set; }
    public int RatingCount { get; set; }
    public string? PrepTime { get; set; }
}

public class CreateMenuItemDto
{
    public string ItemId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string EmojiIcon { get; set; } = "🍲";
    public string? ImageUrl { get; set; }
    public bool IsVegetarian { get; set; } = true;
    public bool IsBestseller { get; set; }
    public bool IsSpicy { get; set; }
    public bool IsKidsChoice { get; set; }
    public string? PrepTime { get; set; } = "10-12 mins";
}

public class UpdateMenuItemAvailabilityDto
{
    public bool IsAvailable { get; set; }
    public bool IsSoldOut { get; set; }
}

public class EmployeeVerificationRequestDto
{
    public string EmployeeId { get; set; } = string.Empty;
    public string DeviceToken { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
}

public class EmployeeVerificationResponseDto
{
    public bool IsEligible { get; set; }
    public decimal ConcessionPercentage { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? EmployeeName { get; set; }
    public string? Designation { get; set; }
    public string? Department { get; set; }
}

public class RegisterEmployeeDto
{
    public string EmployeeId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string? ShiftPolicy { get; set; }
    public string? IdCardUrl { get; set; }
}
