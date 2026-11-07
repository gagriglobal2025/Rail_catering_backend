using Microsoft.EntityFrameworkCore;
using RailCat.Users.Api.Models.Entities;

namespace RailCat.Users.Api.Data;

public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }

    public DbSet<StaffUser> StaffUsers => Set<StaffUser>();
    public DbSet<StaffCredential> StaffCredentials => Set<StaffCredential>();
    public DbSet<StaffSession> StaffSessions => Set<StaffSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);
    }
}

