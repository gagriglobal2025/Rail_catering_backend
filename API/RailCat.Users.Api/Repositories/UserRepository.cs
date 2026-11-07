using Microsoft.EntityFrameworkCore;
using RailCat.Users.Api.Data;
using RailCat.Users.Api.Models.Entities;
using RailCat.Users.Api.Repositories.Interfaces;

namespace RailCat.Users.Api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UsersDbContext _context;

    public UserRepository(UsersDbContext context)
    {
        _context = context;
    }

    public async Task<StaffUser?> GetByIdAsync(Guid id)
    {
        return await _context.StaffUsers
            .Include(u => u.Credential)
            .FirstOrDefaultAsync(u => u.UserId == id);
    }

    public async Task<StaffUser?> GetByEmailOrEmployeeIdAsync(string identifier)
    {
        return await _context.StaffUsers
            .Include(u => u.Credential)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == identifier.ToLower() || u.EmployeeId.ToLower() == identifier.ToLower());
    }

    public async Task<IEnumerable<StaffUser>> GetAllAsync()
    {
        return await _context.StaffUsers
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

    public async Task<StaffUser> AddAsync(StaffUser user)
    {
        await _context.StaffUsers.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(StaffUser user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.StaffUsers.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(StaffUser user)
    {
        _context.StaffUsers.Remove(user);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

