using Microsoft.EntityFrameworkCore;
using RailCat.Settlement.Api.Models.Entities;

namespace RailCat.Settlement.Api.Data;

public class SettlementDbContext : DbContext
{
    public SettlementDbContext(DbContextOptions<SettlementDbContext> options) : base(options) { }

    public DbSet<CashierShift> CashierShifts => Set<CashierShift>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<DailySalesSummary> DailySalesSummaries => Set<DailySalesSummary>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SettlementDbContext).Assembly);
    }
}
