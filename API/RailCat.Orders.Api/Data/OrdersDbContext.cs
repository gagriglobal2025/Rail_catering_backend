using Microsoft.EntityFrameworkCore;
using RailCat.Orders.Api.Models.Entities;

namespace RailCat.Orders.Api.Data;

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

    public DbSet<DailyQueueSequence> DailyQueueSequences => Set<DailyQueueSequence>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<PrintSpool> PrintSpools => Set<PrintSpool>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);
    }
}

