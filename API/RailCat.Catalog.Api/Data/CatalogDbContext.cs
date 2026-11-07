using Microsoft.EntityFrameworkCore;
using RailCat.Catalog.Api.Models.Entities;

namespace RailCat.Catalog.Api.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<ConcessionPolicy> ConcessionPolicies => Set<ConcessionPolicy>();
    public DbSet<RailwayConcessionEmployee> RailwayConcessionEmployees => Set<RailwayConcessionEmployee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }
}
