using RailCat.Catalog.Api.Models.DTOs;
using RailCat.Catalog.Api.Models.Entities;
using RailCat.Catalog.Api.Repositories;

namespace RailCat.Catalog.Api.Services;

public interface IMenuService
{
    Task<IEnumerable<CategoryResponseDto>> GetCategoriesAsync();
    Task<IEnumerable<MenuItemResponseDto>> GetMenuItemsAsync(string? categoryId = null);
    Task<MenuItemResponseDto?> GetMenuItemByIdAsync(string id);
    Task<MenuItemResponseDto> CreateMenuItemAsync(CreateMenuItemDto dto);
    Task<bool> UpdateAvailabilityAsync(string id, UpdateMenuItemAvailabilityDto dto);
}

public interface IConcessionService
{
    Task<EmployeeVerificationResponseDto> VerifyEmployeeAsync(EmployeeVerificationRequestDto dto);
    Task<IEnumerable<RailwayConcessionEmployee>> GetAllEmployeesAsync();
    Task<RailwayConcessionEmployee> RegisterEmployeeAsync(RegisterEmployeeDto dto);
    Task<bool> LockDeviceAsync(string employeeId, string deviceToken, string deviceName);
}

public class MenuService : IMenuService
{
    private readonly IMenuRepository _repo;

    public MenuService(IMenuRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<CategoryResponseDto>> GetCategoriesAsync()
    {
        var cats = await _repo.GetCategoriesAsync();
        return cats.Select(c => new CategoryResponseDto
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName,
            SortOrder = c.SortOrder,
            IsActive = c.IsActive
        });
    }

    public async Task<IEnumerable<MenuItemResponseDto>> GetMenuItemsAsync(string? categoryId = null)
    {
        var items = await _repo.GetMenuItemsAsync(categoryId);
        return items.Select(MapToDto);
    }

    public async Task<MenuItemResponseDto?> GetMenuItemByIdAsync(string id)
    {
        var item = await _repo.GetMenuItemByIdAsync(id);
        return item == null ? null : MapToDto(item);
    }

    public async Task<MenuItemResponseDto> CreateMenuItemAsync(CreateMenuItemDto dto)
    {
        var item = new MenuItem
        {
            ItemId = dto.ItemId,
            CategoryId = dto.CategoryId,
            ItemName = dto.ItemName,
            ItemDescription = dto.ItemDescription,
            BasePrice = dto.BasePrice,
            EmojiIcon = dto.EmojiIcon,
            ImageUrl = dto.ImageUrl,
            IsVegetarian = dto.IsVegetarian,
            IsBestseller = dto.IsBestseller,
            IsSpicy = dto.IsSpicy,
            IsKidsChoice = dto.IsKidsChoice,
            PrepTime = dto.PrepTime,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repo.AddMenuItemAsync(item);
        return MapToDto(created);
    }

    public async Task<bool> UpdateAvailabilityAsync(string id, UpdateMenuItemAvailabilityDto dto)
    {
        var item = await _repo.GetMenuItemByIdAsync(id);
        if (item == null) return false;

        item.IsAvailable = dto.IsAvailable;
        item.IsSoldOut = dto.IsSoldOut;
        await _repo.UpdateMenuItemAsync(item);
        return true;
    }

    private static MenuItemResponseDto MapToDto(MenuItem i) => new()
    {
        ItemId = i.ItemId,
        CategoryId = i.CategoryId,
        ItemName = i.ItemName,
        ItemDescription = i.ItemDescription,
        BasePrice = i.BasePrice,
        EmojiIcon = i.EmojiIcon,
        ImageUrl = i.ImageUrl,
        IsVegetarian = i.IsVegetarian,
        IsAvailable = i.IsAvailable,
        IsSoldOut = i.IsSoldOut,
        IsBestseller = i.IsBestseller,
        IsSpicy = i.IsSpicy,
        IsKidsChoice = i.IsKidsChoice,
        Rating = i.Rating,
        RatingCount = i.RatingCount,
        PrepTime = i.PrepTime
    };
}

public class ConcessionService : IConcessionService
{
    private readonly IEmployeeRepository _repo;

    public ConcessionService(IEmployeeRepository repo)
    {
        _repo = repo;
    }

    public async Task<EmployeeVerificationResponseDto> VerifyEmployeeAsync(EmployeeVerificationRequestDto dto)
    {
        var emp = await _repo.GetByEmployeeIdAsync(dto.EmployeeId);
        var policy = await _repo.GetActiveConcessionPolicyAsync();
        var percent = policy?.DiscountPercentage ?? 20.00m;

        if (emp == null)
        {
            return new EmployeeVerificationResponseDto
            {
                IsEligible = false,
                ConcessionPercentage = 0,
                Message = "Railway Employee ID not found in directory."
            };
        }

        if (emp.VerificationStatus != "ACTIVE")
        {
            return new EmployeeVerificationResponseDto
            {
                IsEligible = false,
                ConcessionPercentage = 0,
                Message = $"Verification is currently {emp.VerificationStatus}. Awaiting Admin UMID approval."
            };
        }

        // Check Device Lock (1 employee = 1 device)
        if (emp.DeviceLocked && !string.IsNullOrEmpty(emp.DeviceToken) && emp.DeviceToken != dto.DeviceToken)
        {
            return new EmployeeVerificationResponseDto
            {
                IsEligible = false,
                ConcessionPercentage = 0,
                Message = $"Employee ID is locked to another device ({emp.DeviceName ?? "registered phone"}). Concession denied."
            };
        }

        // Auto-lock device on first use if not locked
        if (!emp.DeviceLocked)
        {
            emp.DeviceToken = dto.DeviceToken;
            emp.DeviceName = dto.DeviceName ?? "Mobile Device";
            emp.DeviceLocked = true;
            await _repo.UpdateAsync(emp);
        }

        return new EmployeeVerificationResponseDto
        {
            IsEligible = true,
            ConcessionPercentage = percent,
            Message = "Railway Employee Concession 20% Verified.",
            EmployeeName = emp.FullName,
            Designation = emp.Designation,
            Department = emp.Department
        };
    }

    public async Task<IEnumerable<RailwayConcessionEmployee>> GetAllEmployeesAsync()
    {
        return await _repo.GetAllEmployeesAsync();
    }

    public async Task<RailwayConcessionEmployee> RegisterEmployeeAsync(RegisterEmployeeDto dto)
    {
        var emp = new RailwayConcessionEmployee
        {
            EmployeeId = dto.EmployeeId,
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Department = dto.Department,
            Designation = dto.Designation,
            ShiftPolicy = dto.ShiftPolicy ?? "Standard",
            IdCardUrl = dto.IdCardUrl,
            VerificationStatus = "PENDING",
            RegisteredAt = DateTime.UtcNow
        };

        return await _repo.AddAsync(emp);
    }

    public async Task<bool> LockDeviceAsync(string employeeId, string deviceToken, string deviceName)
    {
        var emp = await _repo.GetByEmployeeIdAsync(employeeId);
        if (emp == null) return false;

        emp.DeviceToken = deviceToken;
        emp.DeviceName = deviceName;
        emp.DeviceLocked = true;
        await _repo.UpdateAsync(emp);
        return true;
    }
}

