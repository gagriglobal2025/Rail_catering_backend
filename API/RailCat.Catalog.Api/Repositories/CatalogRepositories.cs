using Microsoft.EntityFrameworkCore;
using RailCat.Catalog.Api.Data;
using RailCat.Catalog.Api.Models.Entities;

namespace RailCat.Catalog.Api.Repositories;

public interface IMenuRepository
{
    Task<IEnumerable<MenuCategory>> GetCategoriesAsync();
    Task<IEnumerable<MenuItem>> GetMenuItemsAsync(string? categoryId = null);
    Task<MenuItem?> GetMenuItemByIdAsync(string id);
    Task<MenuItem> AddMenuItemAsync(MenuItem item);
    Task UpdateMenuItemAsync(MenuItem item);
    Task<bool> DeleteMenuItemAsync(string id);
}

public interface IEmployeeRepository
{
    Task<IEnumerable<RailwayConcessionEmployee>> GetAllEmployeesAsync();
    Task<IEnumerable<RailwayConcessionEmployee>> GetFilteredEmployeesAsync(string? status = null, string? search = null);
    Task<RailwayConcessionEmployee?> GetByEmployeeIdAsync(string employeeId);
    Task<RailwayConcessionEmployee> AddAsync(RailwayConcessionEmployee employee);
    Task UpdateAsync(RailwayConcessionEmployee employee);
    Task<ConcessionPolicy?> GetActiveConcessionPolicyAsync();
    Task<IEnumerable<ConcessionPolicy>> GetAllConcessionPoliciesAsync();
    Task<ConcessionPolicy?> GetConcessionPolicyByIdAsync(string policyId);
    Task UpdateConcessionPolicyAsync(ConcessionPolicy policy);
}

public class MenuRepository : IMenuRepository
{
    private readonly CatalogDbContext _context;

    public MenuRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MenuCategory>> GetCategoriesAsync()
    {
        return await _context.MenuCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<MenuItem>> GetMenuItemsAsync(string? categoryId = null)
    {
        var query = _context.MenuItems.AsQueryable();
        if (!string.IsNullOrEmpty(categoryId))
        {
            query = query.Where(i => i.CategoryId == categoryId);
        }
        return await query.OrderBy(i => i.ItemName).ToListAsync();
    }

    public async Task<MenuItem?> GetMenuItemByIdAsync(string id)
    {
        return await _context.MenuItems.FirstOrDefaultAsync(i => i.ItemId == id);
    }

    public async Task<MenuItem> AddMenuItemAsync(MenuItem item)
    {
        await _context.MenuItems.AddAsync(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task UpdateMenuItemAsync(MenuItem item)
    {
        item.UpdatedAt = DateTime.UtcNow;
        _context.MenuItems.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteMenuItemAsync(string id)
    {
        var item = await _context.MenuItems.FirstOrDefaultAsync(i => i.ItemId == id);
        if (item == null) return false;
        _context.MenuItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }
}

public class EmployeeRepository : IEmployeeRepository
{
    private readonly CatalogDbContext _context;

    public EmployeeRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RailwayConcessionEmployee>> GetAllEmployeesAsync()
    {
        return await _context.RailwayConcessionEmployees
            .OrderBy(e => e.FullName)
            .ToListAsync();
    }

    public async Task<IEnumerable<RailwayConcessionEmployee>> GetFilteredEmployeesAsync(string? status = null, string? search = null)
    {
        var query = _context.RailwayConcessionEmployees.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && status.ToUpper() != "ALL")
        {
            var s = status.Trim().ToUpper();
            query = query.Where(e => e.VerificationStatus == s);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            query = query.Where(e =>
                e.FullName.ToLower().Contains(q) ||
                e.EmployeeId.ToLower().Contains(q) ||
                (e.DeviceName != null && e.DeviceName.ToLower().Contains(q))
            );
        }

        return await query.OrderByDescending(e => e.RegisteredAt).ToListAsync();
    }

    public async Task<RailwayConcessionEmployee?> GetByEmployeeIdAsync(string employeeId)
    {
        return await _context.RailwayConcessionEmployees
            .FirstOrDefaultAsync(e => e.EmployeeId.ToLower() == employeeId.ToLower());
    }

    public async Task<RailwayConcessionEmployee> AddAsync(RailwayConcessionEmployee employee)
    {
        await _context.RailwayConcessionEmployees.AddAsync(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task UpdateAsync(RailwayConcessionEmployee employee)
    {
        _context.RailwayConcessionEmployees.Update(employee);
        await _context.SaveChangesAsync();
    }

    public async Task<ConcessionPolicy?> GetActiveConcessionPolicyAsync()
    {
        return await _context.ConcessionPolicies
            .FirstOrDefaultAsync(p => p.IsActive && p.CustomerType == "RAILWAY_EMPLOYEE");
    }

    public async Task<IEnumerable<ConcessionPolicy>> GetAllConcessionPoliciesAsync()
    {
        return await _context.ConcessionPolicies
            .OrderBy(p => p.PolicyName)
            .ToListAsync();
    }

    public async Task<ConcessionPolicy?> GetConcessionPolicyByIdAsync(string policyId)
    {
        return await _context.ConcessionPolicies
            .FirstOrDefaultAsync(p => p.PolicyId.ToLower() == policyId.ToLower());
    }

    public async Task UpdateConcessionPolicyAsync(ConcessionPolicy policy)
    {
        _context.ConcessionPolicies.Update(policy);
        await _context.SaveChangesAsync();
    }
}

public interface IStationRepository
{
    Task<IEnumerable<Station>> GetAllAsync(bool? isActive = null);
    Task<Station?> GetByIdAsync(string stationId);
    Task<Station> AddAsync(Station station);
    Task UpdateAsync(Station station);
    Task<bool> DeleteAsync(string stationId);
}

public class StationRepository : IStationRepository
{
    private readonly CatalogDbContext _context;

    public StationRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Station>> GetAllAsync(bool? isActive = null)
    {
        var query = _context.Stations.AsQueryable();
        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }
        return await query.OrderBy(s => s.StationName).ToListAsync();
    }

    public async Task<Station?> GetByIdAsync(string stationId)
    {
        return await _context.Stations.FirstOrDefaultAsync(s => s.StationId.ToLower() == stationId.ToLower());
    }

    public async Task<Station> AddAsync(Station station)
    {
        await _context.Stations.AddAsync(station);
        await _context.SaveChangesAsync();
        return station;
    }

    public async Task UpdateAsync(Station station)
    {
        station.UpdatedAt = DateTime.UtcNow;
        _context.Stations.Update(station);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(string stationId)
    {
        var station = await GetByIdAsync(stationId);
        if (station == null) return false;
        _context.Stations.Remove(station);
        await _context.SaveChangesAsync();
        return true;
    }
}

public interface IAdRepository
{
    Task<IEnumerable<PromotionalAd>> GetAllAsync(bool? isActive = null);
    Task<PromotionalAd?> GetByIdAsync(string adId);
    Task<PromotionalAd> AddAsync(PromotionalAd ad);
    Task UpdateAsync(PromotionalAd ad);
    Task<bool> DeleteAsync(string adId);
}

public class AdRepository : IAdRepository
{
    private readonly CatalogDbContext _context;

    public AdRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PromotionalAd>> GetAllAsync(bool? isActive = null)
    {
        var query = _context.PromotionalAds.AsQueryable();
        if (isActive.HasValue)
        {
            query = query.Where(a => a.IsActive == isActive.Value);
        }
        return await query.OrderBy(a => a.SortOrder).ToListAsync();
    }

    public async Task<PromotionalAd?> GetByIdAsync(string adId)
    {
        return await _context.PromotionalAds.FirstOrDefaultAsync(a => a.AdId.ToLower() == adId.ToLower());
    }

    public async Task<PromotionalAd> AddAsync(PromotionalAd ad)
    {
        await _context.PromotionalAds.AddAsync(ad);
        await _context.SaveChangesAsync();
        return ad;
    }

    public async Task UpdateAsync(PromotionalAd ad)
    {
        ad.UpdatedAt = DateTime.UtcNow;
        _context.PromotionalAds.Update(ad);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(string adId)
    {
        var ad = await GetByIdAsync(adId);
        if (ad == null) return false;
        _context.PromotionalAds.Remove(ad);
        await _context.SaveChangesAsync();
        return true;
    }
}

