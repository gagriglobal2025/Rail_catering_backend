using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
    Task<MenuItemResponseDto?> UpdateMenuItemAsync(string id, UpdateMenuItemDto dto);
    Task<bool> UpdateAvailabilityAsync(string id, UpdateMenuItemAvailabilityDto dto);
    Task<bool> DeleteMenuItemAsync(string id);
}

public interface IConcessionService
{
    Task<EmployeeVerificationResponseDto> VerifyEmployeeAsync(EmployeeVerificationRequestDto dto);
    Task<IEnumerable<RailwayEmployeeResponseDto>> GetAllEmployeesAsync(string? status = null, string? search = null);
    Task<RailwayEmployeeResponseDto?> GetEmployeeByIdAsync(string employeeId);
    Task<RailwayEmployeeResponseDto> RegisterEmployeeAsync(RegisterEmployeeDto dto);
    Task<string> UploadIdCardImageAsync(IFormFile file, string employeeId);
    Task<RailwayEmployeeResponseDto?> UpdateEmployeeStatusAsync(string employeeId, EmployeeStatusUpdateDto dto);
    Task<RailwayEmployeeResponseDto?> ToggleDeviceLockAsync(string employeeId, bool? deviceLocked = null);
    Task<bool> LockDeviceAsync(string employeeId, string deviceToken, string deviceName);

    // Concession Policies
    Task<IEnumerable<ConcessionPolicyDto>> GetPoliciesAsync();
    Task<ConcessionPolicyDto?> GetPolicyByIdAsync(string policyId);
    Task<ConcessionPolicyDto?> UpdatePolicyAsync(string policyId, UpdateConcessionPolicyDto dto);
}

public class MenuService : IMenuService
{
    private readonly IMenuRepository _repo;
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MenuService(IMenuRepository repo, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    {
        _repo = repo;
        _env = env;
        _httpContextAccessor = httpContextAccessor;
    }

    private async Task<string?> ProcessImageAsync(IFormFile? file, string? providedUrl, string itemId)
    {
        if (file != null && file.Length > 0)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".avif" };
            if (!allowed.Contains(ext))
            {
                throw new ArgumentException($"Invalid image extension '{ext}'. Allowed extensions: {string.Join(", ", allowed)}");
            }

            if (file.Length > 10 * 1024 * 1024)
            {
                throw new ArgumentException("Image file size exceeds maximum limit of 10 MB.");
            }

            var webRoot = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
            }

            var uploadsFolder = Path.Combine(webRoot, "uploads", "dishes");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var sanitizedId = string.Join("_", itemId.Split(Path.GetInvalidFileNameChars()));
            var uniqueFileName = $"{sanitizedId}_{Guid.NewGuid().ToString("N")[..8]}{ext}";
            var fullPath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/dishes/{uniqueFileName}";
        }

        return string.IsNullOrWhiteSpace(providedUrl) ? null : providedUrl.Trim();
    }

    private async Task<string> GenerateUniqueItemIdAsync(string itemName)
    {
        var baseSlug = Regex.Replace(itemName.Trim().ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
        if (string.IsNullOrWhiteSpace(baseSlug))
        {
            baseSlug = "item";
        }

        if (baseSlug.Length > 40)
        {
            baseSlug = baseSlug[..40].Trim('-');
        }

        var existing = await _repo.GetMenuItemByIdAsync(baseSlug);
        if (existing == null)
        {
            return baseSlug;
        }

        while (true)
        {
            var suffix = Guid.NewGuid().ToString("N")[..4];
            var candidate = $"{baseSlug}-{suffix}";
            if (await _repo.GetMenuItemByIdAsync(candidate) == null)
            {
                return candidate;
            }
        }
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
        var finalItemId = await GenerateUniqueItemIdAsync(dto.ItemName);
        var finalImageUrl = await ProcessImageAsync(dto.ImageFile, dto.ImageUrl, finalItemId);

        var item = new MenuItem
        {
            ItemId = finalItemId,
            CategoryId = dto.CategoryId,
            ItemName = dto.ItemName,
            ItemDescription = dto.ItemDescription,
            BasePrice = dto.BasePrice,
            EmojiIcon = dto.EmojiIcon,
            ImageUrl = finalImageUrl,
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

    public async Task<MenuItemResponseDto?> UpdateMenuItemAsync(string id, UpdateMenuItemDto dto)
    {
        var item = await _repo.GetMenuItemByIdAsync(id);
        if (item == null) return null;

        // Partial update coalescing: only update fields that were provided
        if (!string.IsNullOrWhiteSpace(dto.CategoryId)) item.CategoryId = dto.CategoryId;
        if (!string.IsNullOrWhiteSpace(dto.ItemName)) item.ItemName = dto.ItemName;
        if (dto.ItemDescription != null) item.ItemDescription = dto.ItemDescription;
        if (dto.BasePrice.HasValue) item.BasePrice = dto.BasePrice.Value;
        if (!string.IsNullOrWhiteSpace(dto.EmojiIcon)) item.EmojiIcon = dto.EmojiIcon;
        if (dto.IsVegetarian.HasValue) item.IsVegetarian = dto.IsVegetarian.Value;
        if (dto.IsAvailable.HasValue) item.IsAvailable = dto.IsAvailable.Value;
        if (dto.IsSoldOut.HasValue) item.IsSoldOut = dto.IsSoldOut.Value;
        if (dto.IsBestseller.HasValue) item.IsBestseller = dto.IsBestseller.Value;
        if (dto.IsSpicy.HasValue) item.IsSpicy = dto.IsSpicy.Value;
        if (dto.IsKidsChoice.HasValue) item.IsKidsChoice = dto.IsKidsChoice.Value;
        if (!string.IsNullOrWhiteSpace(dto.PrepTime)) item.PrepTime = dto.PrepTime;

        // Process file upload if provided
        if (dto.ImageFile != null && dto.ImageFile.Length > 0)
        {
            item.ImageUrl = await ProcessImageAsync(dto.ImageFile, null, id);
        }
        else if (dto.ImageUrl != null)
        {
            // If explicit URL string is passed, update it (or clear if empty string)
            item.ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim();
        }

        item.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateMenuItemAsync(item);
        return MapToDto(item);
    }

    public async Task<bool> DeleteMenuItemAsync(string id)
    {
        return await _repo.DeleteMenuItemAsync(id);
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

    private string? ResolveImageUrl(string? rawUrl)
    {
        if (string.IsNullOrWhiteSpace(rawUrl)) return null;

        // If it's already an absolute external URL, preserve as-is
        if (rawUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            rawUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return rawUrl;
        }

        // Auto-prepend current host/base path for relative /uploads/ URLs
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var req = httpContext.Request;
            var scheme = req.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? req.Scheme;
            var host = req.Headers["X-Forwarded-Host"].FirstOrDefault() ?? req.Host.Value;
            var pathBase = req.PathBase.Value?.TrimEnd('/') ?? "";
            var cleanPath = rawUrl.StartsWith('/') ? rawUrl : "/" + rawUrl;
            return $"{scheme}://{host}{pathBase}{cleanPath}";
        }

        return rawUrl;
    }

    private MenuItemResponseDto MapToDto(MenuItem i) => new()
    {
        ItemId = i.ItemId,
        CategoryId = i.CategoryId,
        ItemName = i.ItemName,
        ItemDescription = i.ItemDescription,
        BasePrice = i.BasePrice,
        EmojiIcon = i.EmojiIcon,
        ImageUrl = ResolveImageUrl(i.ImageUrl),
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
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ConcessionService(IEmployeeRepository repo, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    {
        _repo = repo;
        _env = env;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> UploadIdCardImageAsync(IFormFile file, string employeeId)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowed.Contains(ext))
        {
            throw new ArgumentException($"Invalid image extension '{ext}'. Allowed extensions: {string.Join(", ", allowed)}");
        }

        if (file.Length > 10 * 1024 * 1024)
        {
            throw new ArgumentException("Image file size exceeds maximum limit of 10 MB.");
        }

        var webRoot = _env.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
        {
            webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
        }

        var uploadsFolder = Path.Combine(webRoot, "uploads", "id-cards");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var sanitizedId = string.Join("_", employeeId.Split(Path.GetInvalidFileNameChars()));
        var uniqueFileName = $"{sanitizedId}_{Guid.NewGuid().ToString("N")[..8]}{ext}";
        var fullPath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return ResolveImageUrl($"/uploads/id-cards/{uniqueFileName}")!;
    }

    private string? ResolveImageUrl(string? rawUrl)
    {
        if (string.IsNullOrWhiteSpace(rawUrl)) return null;

        if (rawUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            rawUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return rawUrl;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var req = httpContext.Request;
            var scheme = req.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? req.Scheme;
            var host = req.Headers["X-Forwarded-Host"].FirstOrDefault() ?? req.Host.Value;
            var pathBase = req.PathBase.Value?.TrimEnd('/') ?? "";
            var cleanPath = rawUrl.StartsWith('/') ? rawUrl : "/" + rawUrl;
            return $"{scheme}://{host}{pathBase}{cleanPath}";
        }

        return rawUrl;
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
            EmployeeName = emp.FullName
        };
    }

    public async Task<IEnumerable<RailwayEmployeeResponseDto>> GetAllEmployeesAsync(string? status = null, string? search = null)
    {
        var emps = await _repo.GetFilteredEmployeesAsync(status, search);
        return emps.Select(MapEmployeeToDto);
    }

    public async Task<RailwayEmployeeResponseDto?> GetEmployeeByIdAsync(string employeeId)
    {
        var emp = await _repo.GetByEmployeeIdAsync(employeeId);
        return emp == null ? null : MapEmployeeToDto(emp);
    }

    public async Task<RailwayEmployeeResponseDto> RegisterEmployeeAsync(RegisterEmployeeDto dto)
    {
        var existing = await _repo.GetByEmployeeIdAsync(dto.EmployeeId);

        // Re-submission (e.g. "Edit application / Re-upload ID") refreshes the existing
        // application instead of failing on the employee_id primary key.
        if (existing != null)
        {
            existing.FullName = dto.FullName;
            existing.Phone = dto.Phone;
            if (!string.IsNullOrWhiteSpace(dto.IdCardUrl)) existing.IdCardUrl = dto.IdCardUrl;
            if (!string.IsNullOrWhiteSpace(dto.DeviceToken)) existing.DeviceToken = dto.DeviceToken;
            if (!string.IsNullOrWhiteSpace(dto.DeviceName)) existing.DeviceName = dto.DeviceName;

            if (existing.VerificationStatus != "ACTIVE")
            {
                existing.VerificationStatus = "PENDING";
                existing.RejectionReason = null;
            }

            await _repo.UpdateAsync(existing);
            return MapEmployeeToDto(existing);
        }

        var emp = new RailwayConcessionEmployee
        {
            EmployeeId = dto.EmployeeId.Trim(),
            FullName = dto.FullName,
            Phone = dto.Phone,
            DeviceToken = dto.DeviceToken,
            DeviceName = dto.DeviceName,
            IdCardUrl = dto.IdCardUrl,
            VerificationStatus = "PENDING",
            RegisteredAt = DateTime.UtcNow
        };

        var created = await _repo.AddAsync(emp);
        return MapEmployeeToDto(created);
    }

    public async Task<RailwayEmployeeResponseDto?> UpdateEmployeeStatusAsync(string employeeId, EmployeeStatusUpdateDto dto)
    {
        var emp = await _repo.GetByEmployeeIdAsync(employeeId);
        if (emp == null) return null;

        var status = dto.Status.Trim().ToUpper();
        emp.VerificationStatus = status;

        if (status == "ACTIVE")
        {
            emp.VerifiedAt = DateTime.UtcNow;
            // Only lock if a device was actually captured at registration/re-submission;
            // otherwise leave it open so the first verify-concession call can bind it.
            emp.DeviceLocked = !string.IsNullOrEmpty(emp.DeviceToken);
            emp.RejectionReason = null;
        }
        else if (status == "REJECTED")
        {
            emp.RejectionReason = dto.RejectionReason;
        }

        await _repo.UpdateAsync(emp);
        return MapEmployeeToDto(emp);
    }

    public async Task<RailwayEmployeeResponseDto?> ToggleDeviceLockAsync(string employeeId, bool? deviceLocked = null)
    {
        var emp = await _repo.GetByEmployeeIdAsync(employeeId);
        if (emp == null) return null;

        emp.DeviceLocked = deviceLocked ?? !emp.DeviceLocked;
        await _repo.UpdateAsync(emp);
        return MapEmployeeToDto(emp);
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

    public async Task<IEnumerable<ConcessionPolicyDto>> GetPoliciesAsync()
    {
        var policies = await _repo.GetAllConcessionPoliciesAsync();
        return policies.Select(p => new ConcessionPolicyDto
        {
            PolicyId = p.PolicyId,
            PolicyName = p.PolicyName,
            CustomerType = p.CustomerType,
            DiscountPercentage = p.DiscountPercentage,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt
        });
    }

    public async Task<ConcessionPolicyDto?> GetPolicyByIdAsync(string policyId)
    {
        var p = await _repo.GetConcessionPolicyByIdAsync(policyId);
        if (p == null) return null;

        return new ConcessionPolicyDto
        {
            PolicyId = p.PolicyId,
            PolicyName = p.PolicyName,
            CustomerType = p.CustomerType,
            DiscountPercentage = p.DiscountPercentage,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt
        };
    }

    public async Task<ConcessionPolicyDto?> UpdatePolicyAsync(string policyId, UpdateConcessionPolicyDto dto)
    {
        var policy = await _repo.GetConcessionPolicyByIdAsync(policyId);
        if (policy == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.PolicyName))
        {
            policy.PolicyName = dto.PolicyName.Trim();
        }

        if (dto.DiscountPercentage.HasValue)
        {
            policy.DiscountPercentage = dto.DiscountPercentage.Value;
        }

        if (dto.IsActive.HasValue)
        {
            policy.IsActive = dto.IsActive.Value;
        }

        await _repo.UpdateConcessionPolicyAsync(policy);

        return new ConcessionPolicyDto
        {
            PolicyId = policy.PolicyId,
            PolicyName = policy.PolicyName,
            CustomerType = policy.CustomerType,
            DiscountPercentage = policy.DiscountPercentage,
            IsActive = policy.IsActive,
            CreatedAt = policy.CreatedAt
        };
    }

    private RailwayEmployeeResponseDto MapEmployeeToDto(RailwayConcessionEmployee e) => new()
    {
        EmployeeId = e.EmployeeId,
        FullName = e.FullName,
        Phone = e.Phone,
        VerificationStatus = e.VerificationStatus,
        DeviceName = e.DeviceName,
        DeviceToken = e.DeviceToken,
        DeviceLocked = e.DeviceLocked,
        IdCardUrl = ResolveImageUrl(e.IdCardUrl),
        RejectionReason = e.RejectionReason,
        RegisteredAt = e.RegisteredAt,
        VerifiedAt = e.VerifiedAt
    };
}

public interface IStationService
{
    Task<IEnumerable<StationResponseDto>> GetAllAsync(bool? isActive = null);
    Task<StationResponseDto?> GetByIdAsync(string stationId);
    Task<StationResponseDto> CreateAsync(CreateStationDto dto);
    Task<StationResponseDto?> UpdateAsync(string stationId, UpdateStationDto dto);
    Task<bool> DeleteAsync(string stationId);
}

public class StationService : IStationService
{
    private readonly IStationRepository _repo;

    public StationService(IStationRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<StationResponseDto>> GetAllAsync(bool? isActive = null)
    {
        var stations = await _repo.GetAllAsync(isActive);
        return stations.Select(MapToDto);
    }

    public async Task<StationResponseDto?> GetByIdAsync(string stationId)
    {
        var station = await _repo.GetByIdAsync(stationId);
        return station == null ? null : MapToDto(station);
    }

    public async Task<StationResponseDto> CreateAsync(CreateStationDto dto)
    {
        var station = new Station
        {
            StationId = dto.StationId.Trim().ToUpperInvariant(),
            StationName = dto.StationName,
            StationCode = dto.StationCode.Trim().ToUpperInvariant(),
            Platform = dto.Platform,
            Address = dto.Address,
            GeofenceRadiusM = dto.GeofenceRadiusM ?? 250,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repo.AddAsync(station);
        return MapToDto(created);
    }

    public async Task<StationResponseDto?> UpdateAsync(string stationId, UpdateStationDto dto)
    {
        var station = await _repo.GetByIdAsync(stationId);
        if (station == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.StationName)) station.StationName = dto.StationName;
        if (!string.IsNullOrWhiteSpace(dto.StationCode)) station.StationCode = dto.StationCode.Trim().ToUpperInvariant();
        if (dto.Platform != null) station.Platform = dto.Platform;
        if (dto.Address != null) station.Address = dto.Address;
        if (dto.GeofenceRadiusM.HasValue) station.GeofenceRadiusM = dto.GeofenceRadiusM.Value;
        if (dto.IsActive.HasValue) station.IsActive = dto.IsActive.Value;

        await _repo.UpdateAsync(station);
        return MapToDto(station);
    }

    public async Task<bool> DeleteAsync(string stationId)
    {
        return await _repo.DeleteAsync(stationId);
    }

    private static StationResponseDto MapToDto(Station s) => new()
    {
        StationId = s.StationId,
        StationName = s.StationName,
        StationCode = s.StationCode,
        Platform = s.Platform,
        Address = s.Address,
        GeofenceRadiusM = s.GeofenceRadiusM,
        IsActive = s.IsActive,
        CreatedAt = s.CreatedAt
    };
}

public interface IAdService
{
    Task<IEnumerable<AdResponseDto>> GetAllAsync(bool? isActive = null);
    Task<AdResponseDto?> GetByIdAsync(string adId);
    Task<AdResponseDto> CreateAsync(CreateAdDto dto);
    Task<AdResponseDto?> UpdateAsync(string adId, UpdateAdDto dto);
    Task<bool> DeleteAsync(string adId);
}

public class AdService : IAdService
{
    private readonly IAdRepository _repo;
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AdService(IAdRepository repo, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    {
        _repo = repo;
        _env = env;
        _httpContextAccessor = httpContextAccessor;
    }

    private async Task<string?> ProcessImageAsync(IFormFile? file, string? providedUrl, string adId)
    {
        if (file != null && file.Length > 0)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            if (!allowed.Contains(ext))
            {
                throw new ArgumentException($"Invalid image extension '{ext}'. Allowed extensions: {string.Join(", ", allowed)}");
            }

            if (file.Length > 10 * 1024 * 1024)
            {
                throw new ArgumentException("Image file size exceeds maximum limit of 10 MB.");
            }

            var webRoot = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
            }

            var uploadsFolder = Path.Combine(webRoot, "uploads", "ads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var sanitizedId = string.Join("_", adId.Split(Path.GetInvalidFileNameChars()));
            var uniqueFileName = $"{sanitizedId}_{Guid.NewGuid().ToString("N")[..8]}{ext}";
            var fullPath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/ads/{uniqueFileName}";
        }

        return string.IsNullOrWhiteSpace(providedUrl) ? null : providedUrl.Trim();
    }

    private string? ResolveImageUrl(string? rawUrl)
    {
        if (string.IsNullOrWhiteSpace(rawUrl)) return null;

        if (rawUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            rawUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return rawUrl;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var req = httpContext.Request;
            var scheme = req.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? req.Scheme;
            var host = req.Headers["X-Forwarded-Host"].FirstOrDefault() ?? req.Host.Value;
            var pathBase = req.PathBase.Value?.TrimEnd('/') ?? "";
            var cleanPath = rawUrl.StartsWith('/') ? rawUrl : "/" + rawUrl;
            return $"{scheme}://{host}{pathBase}{cleanPath}";
        }

        return rawUrl;
    }

    private static string GenerateAdId(string sponsor)
    {
        var baseSlug = Regex.Replace(sponsor.Trim().ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
        if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "ad";
        if (baseSlug.Length > 30) baseSlug = baseSlug[..30].Trim('-');
        return $"{baseSlug}-{Guid.NewGuid().ToString("N")[..6]}";
    }

    public async Task<IEnumerable<AdResponseDto>> GetAllAsync(bool? isActive = null)
    {
        var ads = await _repo.GetAllAsync(isActive);
        return ads.Select(MapToDto);
    }

    public async Task<AdResponseDto?> GetByIdAsync(string adId)
    {
        var ad = await _repo.GetByIdAsync(adId);
        return ad == null ? null : MapToDto(ad);
    }

    public async Task<AdResponseDto> CreateAsync(CreateAdDto dto)
    {
        var adId = string.IsNullOrWhiteSpace(dto.AdId) ? GenerateAdId(dto.Sponsor) : dto.AdId.Trim();
        var imageUrl = await ProcessImageAsync(dto.ImageFile, dto.ImageUrl, adId);

        var ad = new PromotionalAd
        {
            AdId = adId,
            Tag = dto.Tag,
            Sponsor = dto.Sponsor,
            Title = dto.Title,
            Subtitle = dto.Subtitle,
            CtaText = dto.CtaText,
            Href = dto.Href,
            BadgeColor = dto.BadgeColor,
            BgGradient = dto.BgGradient,
            ImageUrl = imageUrl,
            SortOrder = dto.SortOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repo.AddAsync(ad);
        return MapToDto(created);
    }

    public async Task<AdResponseDto?> UpdateAsync(string adId, UpdateAdDto dto)
    {
        var ad = await _repo.GetByIdAsync(adId);
        if (ad == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Tag)) ad.Tag = dto.Tag;
        if (!string.IsNullOrWhiteSpace(dto.Sponsor)) ad.Sponsor = dto.Sponsor;
        if (!string.IsNullOrWhiteSpace(dto.Title)) ad.Title = dto.Title;
        if (dto.Subtitle != null) ad.Subtitle = dto.Subtitle;
        if (!string.IsNullOrWhiteSpace(dto.CtaText)) ad.CtaText = dto.CtaText;
        if (dto.Href != null) ad.Href = dto.Href;
        if (!string.IsNullOrWhiteSpace(dto.BadgeColor)) ad.BadgeColor = dto.BadgeColor;
        if (dto.BgGradient != null) ad.BgGradient = dto.BgGradient;
        if (dto.SortOrder.HasValue) ad.SortOrder = dto.SortOrder.Value;
        if (dto.IsActive.HasValue) ad.IsActive = dto.IsActive.Value;

        if (dto.ImageFile != null && dto.ImageFile.Length > 0)
        {
            ad.ImageUrl = await ProcessImageAsync(dto.ImageFile, null, adId);
        }
        else if (dto.ImageUrl != null)
        {
            ad.ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim();
        }

        await _repo.UpdateAsync(ad);
        return MapToDto(ad);
    }

    public async Task<bool> DeleteAsync(string adId)
    {
        return await _repo.DeleteAsync(adId);
    }

    private AdResponseDto MapToDto(PromotionalAd a) => new()
    {
        AdId = a.AdId,
        Tag = a.Tag,
        Sponsor = a.Sponsor,
        Title = a.Title,
        Subtitle = a.Subtitle,
        CtaText = a.CtaText,
        Href = a.Href,
        BadgeColor = a.BadgeColor,
        BgGradient = a.BgGradient,
        ImageUrl = ResolveImageUrl(a.ImageUrl),
        SortOrder = a.SortOrder,
        IsActive = a.IsActive,
        CreatedAt = a.CreatedAt
    };
}

