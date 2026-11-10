using Microsoft.AspNetCore.Http;

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
    public string CategoryId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string EmojiIcon { get; set; } = "🍲";
    public string? ImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
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

public class UpdateMenuItemDto
{
    public string? CategoryId { get; set; }
    public string? ItemName { get; set; }
    public string? ItemDescription { get; set; }
    public decimal? BasePrice { get; set; }
    public string? EmojiIcon { get; set; }
    public string? ImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
    public bool? IsVegetarian { get; set; }
    public bool? IsAvailable { get; set; }
    public bool? IsSoldOut { get; set; }
    public bool? IsBestseller { get; set; }
    public bool? IsSpicy { get; set; }
    public bool? IsKidsChoice { get; set; }
    public string? PrepTime { get; set; }
}

public class ConcessionPolicyDto
{
    public string PolicyId { get; set; } = string.Empty;
    public string PolicyName { get; set; } = string.Empty;
    public string CustomerType { get; set; } = "RAILWAY_EMPLOYEE";
    public decimal DiscountPercentage { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateConcessionPolicyDto
{
    public string? PolicyName { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public bool? IsActive { get; set; }
}

public class EmployeeStatusUpdateDto
{
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, REJECTED, PENDING
    public string? RejectionReason { get; set; }
}

public class EmployeeDeviceLockDto
{
    public bool? DeviceLocked { get; set; }
}

public class RailwayEmployeeResponseDto
{
    public string EmployeeId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string ShiftPolicy { get; set; } = "Standard";
    public string VerificationStatus { get; set; } = "PENDING";
    public string? DeviceName { get; set; }
    public string? DeviceToken { get; set; }
    public bool DeviceLocked { get; set; }
    public string? IdCardUrl { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

public class EmployeeVerificationRequestDto
{
    public string EmployeeId { get; set; } = string.Empty;
    public string DeviceToken { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public string? VerificationPin { get; set; }
}

public class EmployeeVerificationResponseDto
{
    public bool IsEligible { get; set; }
    public bool IsVerified => IsEligible;
    public decimal ConcessionPercentage { get; set; }
    public decimal DiscountPercentage => ConcessionPercentage;
    public string Message { get; set; } = string.Empty;
    public string? EmployeeName { get; set; }
    public string? FullName => EmployeeName;
    public string? Designation { get; set; }
    public string? Department { get; set; }
    public string StationId { get; set; } = "SC";
    public int DailyQuotaRemaining { get; set; } = 2;
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

