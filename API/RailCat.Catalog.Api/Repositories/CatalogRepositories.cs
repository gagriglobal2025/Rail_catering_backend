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
}

public interface IEmployeeRepository
{
    Task<IEnumerable<RailwayConcessionEmployee>> GetAllEmployeesAsync();
    Task<RailwayConcessionEmployee?> GetByEmployeeIdAsync(string employeeId);
    Task<RailwayConcessionEmployee> AddAsync(RailwayConcessionEmployee employee);
    Task UpdateAsync(RailwayConcessionEmployee employee);
    Task<ConcessionPolicy?> GetActiveConcessionPolicyAsync();
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
}

