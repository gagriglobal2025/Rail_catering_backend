using RailCat.Users.Api.Models.Entities;

namespace RailCat.Users.Api.Repositories.Interfaces;

public interface IUserRepository
{
    Task<StaffUser?> GetByIdAsync(Guid id);
    Task<StaffUser?> GetByIdentifierAsync(string identifier);
    Task<IEnumerable<StaffUser>> GetAllAsync();
    Task<StaffUser> AddAsync(StaffUser user);
    Task UpdateAsync(StaffUser user);
    Task DeleteAsync(StaffUser user);
    Task UpdatePasswordAsync(Guid userId, string newPasswordHash);
    Task ClearUserSessionsAsync(Guid userId);
    Task SaveChangesAsync();
}
