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

    public async Task<StaffUser?> GetByIdentifierAsync(string identifier)
    {
        var cleanInput = identifier.Trim().ToLower();
        var digitsOnly = new string(cleanInput.Where(char.IsDigit).ToArray());

        return await _context.StaffUsers
            .Include(u => u.Credential)
            .FirstOrDefaultAsync(u =>
                u.Email.ToLower() == cleanInput ||
                u.EmployeeId.ToLower() == cleanInput ||
                (u.PhoneNumber != null && (
                    u.PhoneNumber == cleanInput ||
                    (digitsOnly.Length >= 10 && u.PhoneNumber.EndsWith(digitsOnly.Substring(digitsOnly.Length - 10)))
                ))
            );
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

    public async Task UpdatePasswordAsync(Guid userId, string newPasswordHash)
    {
        var cred = await _context.StaffCredentials.FirstOrDefaultAsync(c => c.UserId == userId);
        if (cred != null)
        {
            cred.PasswordHash = newPasswordHash;
            cred.FailedLogins = 0;
            cred.LockedUntil = null;
            cred.PasswordUpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task ClearUserSessionsAsync(Guid userId)
    {
        var sessions = await _context.StaffSessions.Where(s => s.UserId == userId).ToListAsync();
        if (sessions.Any())
        {
            _context.StaffSessions.RemoveRange(sessions);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
