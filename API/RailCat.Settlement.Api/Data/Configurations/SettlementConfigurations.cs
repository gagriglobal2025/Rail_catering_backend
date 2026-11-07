using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RailCat.Settlement.Api.Models.Entities;

namespace RailCat.Settlement.Api.Data.Configurations;

public class CashierShiftConfiguration : IEntityTypeConfiguration<CashierShift>
{
    public void Configure(EntityTypeBuilder<CashierShift> builder)
    {
        builder.ToTable("cashier_shifts");
        builder.HasKey(s => s.ShiftId);
        builder.Property(s => s.ShiftId).HasColumnName("shift_id");
        builder.Property(s => s.CashierUserId).HasColumnName("cashier_user_id");
        builder.Property(s => s.OpenedAt).HasColumnName("opened_at");
        builder.Property(s => s.ClosedAt).HasColumnName("closed_at");
        builder.Property(s => s.StartingCash).HasColumnName("starting_cash").HasColumnType("numeric(10,2)").HasDefaultValue(500.00m);
        builder.Property(s => s.CashCollected).HasColumnName("cash_collected").HasColumnType("numeric(10,2)").HasDefaultValue(0.00m);
        builder.Property(s => s.UpiCollected).HasColumnName("upi_collected").HasColumnType("numeric(10,2)").HasDefaultValue(0.00m);
        builder.Property(s => s.CardCollected).HasColumnName("card_collected").HasColumnType("numeric(10,2)").HasDefaultValue(0.00m);
        builder.Property(s => s.TotalExpected).HasColumnName("total_expected").HasColumnType("numeric(10,2)").HasDefaultValue(500.00m);
        builder.Property(s => s.ActualCashCounted).HasColumnName("actual_cash_counted").HasColumnType("numeric(10,2)");
        builder.Property(s => s.ShiftStatus).HasColumnName("shift_status").HasMaxLength(20).HasDefaultValue("OPEN");

        builder.HasMany(s => s.Transactions)
               .WithOne(t => t.Shift)
               .HasForeignKey(t => t.ShiftId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("payment_transactions");
        builder.HasKey(t => t.PaymentId);
        builder.Property(t => t.PaymentId).HasColumnName("payment_id");
        builder.Property(t => t.OrderId).HasColumnName("order_id");
        builder.Property(t => t.ShiftId).HasColumnName("shift_id");
        builder.Property(t => t.Amount).HasColumnName("amount").HasColumnType("numeric(10,2)");
        builder.Property(t => t.PaymentMethod).HasColumnName("payment_method").HasMaxLength(20);
        builder.Property(t => t.GatewayReference).HasColumnName("gateway_reference").HasMaxLength(100);
        builder.Property(t => t.PaymentStatus).HasColumnName("payment_status").HasMaxLength(20).HasDefaultValue("SUCCESS");
        builder.Property(t => t.CreatedAt).HasColumnName("created_at");
    }
}

public class DailySalesSummaryConfiguration : IEntityTypeConfiguration<DailySalesSummary>
{
    public void Configure(EntityTypeBuilder<DailySalesSummary> builder)
    {
        builder.ToTable("daily_sales_summary");
        builder.HasKey(d => d.SummaryDate);
        builder.Property(d => d.SummaryDate).HasColumnName("summary_date");
        builder.Property(d => d.TotalOrders).HasColumnName("total_orders").HasDefaultValue(0);
        builder.Property(d => d.GrossSales).HasColumnName("gross_sales").HasColumnType("numeric(10,2)").HasDefaultValue(0.00m);
        builder.Property(d => d.ConcessionsGiven).HasColumnName("concessions_given").HasColumnType("numeric(10,2)").HasDefaultValue(0.00m);
        builder.Property(d => d.TaxCollected).HasColumnName("tax_collected").HasColumnType("numeric(10,2)").HasDefaultValue(0.00m);
        builder.Property(d => d.NetRevenue).HasColumnName("net_revenue").HasColumnType("numeric(10,2)").HasDefaultValue(0.00m);
        builder.Property(d => d.UpiRevenue).HasColumnName("upi_revenue").HasColumnType("numeric(10,2)").HasDefaultValue(0.00m);
        builder.Property(d => d.CashRevenue).HasColumnName("cash_revenue").HasColumnType("numeric(10,2)").HasDefaultValue(0.00m);
        builder.Property(d => d.CardRevenue).HasColumnName("card_revenue").HasColumnType("numeric(10,2)").HasDefaultValue(0.00m);
        builder.Property(d => d.UpdatedAt).HasColumnName("updated_at");
    }
}

