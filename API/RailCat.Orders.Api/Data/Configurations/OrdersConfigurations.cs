using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RailCat.Orders.Api.Models.Entities;

namespace RailCat.Orders.Api.Data.Configurations;

public class DailyQueueSequenceConfiguration : IEntityTypeConfiguration<DailyQueueSequence>
{
    public void Configure(EntityTypeBuilder<DailyQueueSequence> builder)
    {
        builder.ToTable("daily_queue_sequence");
        builder.HasKey(q => q.OperationalDate);
        builder.Property(q => q.OperationalDate).HasColumnName("operational_date");
        builder.Property(q => q.CurrentNumber).HasColumnName("current_number").HasDefaultValue(0);
        builder.Property(q => q.TokenPrefix).HasColumnName("token_prefix").HasMaxLength(10).HasDefaultValue("A");
        builder.Property(q => q.UpdatedAt).HasColumnName("updated_at");
    }
}

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(o => o.OrderId);
        builder.Property(o => o.OrderId).HasColumnName("order_id");
        builder.Property(o => o.OrderNumber).HasColumnName("order_number").HasMaxLength(30).IsRequired();
        builder.HasIndex(o => o.OrderNumber).IsUnique();

        builder.Property(o => o.QueueToken).HasColumnName("queue_token").HasMaxLength(20).IsRequired();
        builder.Property(o => o.CustomerType).HasColumnName("customer_type").HasMaxLength(30).IsRequired();
        builder.Property(o => o.CustomerRef).HasColumnName("customer_ref").HasMaxLength(50);
        builder.Property(o => o.PickupCounter).HasColumnName("pickup_counter").HasMaxLength(50).HasDefaultValue("Counter 1");
        builder.Property(o => o.SpecialInstructions).HasColumnName("special_instructions");

        builder.Property(o => o.SubtotalAmount).HasColumnName("subtotal_amount").HasColumnType("numeric(10,2)");
        builder.Property(o => o.ConcessionDiscount).HasColumnName("concession_discount").HasColumnType("numeric(10,2)").HasDefaultValue(0.00m);
        builder.Property(o => o.TaxAmount).HasColumnName("tax_amount").HasColumnType("numeric(10,2)").HasDefaultValue(0.00m);
        builder.Property(o => o.GrandTotal).HasColumnName("grand_total").HasColumnType("numeric(10,2)");

        builder.Property(o => o.OrderStatus).HasColumnName("order_status").HasMaxLength(30).HasDefaultValue("NEW");
        builder.Property(o => o.PaymentStatus).HasColumnName("payment_status").HasMaxLength(30).HasDefaultValue("PENDING");
        builder.Property(o => o.PaymentMethod).HasColumnName("payment_method").HasMaxLength(20);
        builder.Property(o => o.CreatedByStaffId).HasColumnName("created_by_staff_id");

        builder.Property(o => o.PlacedAt).HasColumnName("placed_at");
        builder.Property(o => o.PaidAt).HasColumnName("paid_at");
        builder.Property(o => o.PreparingAt).HasColumnName("preparing_at");
        builder.Property(o => o.ReadyAt).HasColumnName("ready_at");
        builder.Property(o => o.DeliveredAt).HasColumnName("delivered_at");
        builder.Property(o => o.CancelledAt).HasColumnName("cancelled_at");
        builder.Property(o => o.CreatedAt).HasColumnName("created_at");
        builder.Property(o => o.UpdatedAt).HasColumnName("updated_at");

        builder.HasMany(o => o.Items)
               .WithOne(i => i.Order)
               .HasForeignKey(i => i.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.PrintJobs)
               .WithOne(p => p.Order)
               .HasForeignKey(p => p.OrderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");
        builder.HasKey(i => i.OrderItemId);
        builder.Property(i => i.OrderItemId).HasColumnName("order_item_id");
        builder.Property(i => i.OrderId).HasColumnName("order_id");
        builder.Property(i => i.ItemId).HasColumnName("item_id").HasMaxLength(50).IsRequired();
        builder.Property(i => i.ItemName).HasColumnName("item_name").HasMaxLength(100).IsRequired();
        builder.Property(i => i.Quantity).HasColumnName("quantity");
        builder.Property(i => i.UnitPrice).HasColumnName("unit_price").HasColumnType("numeric(10,2)");
        builder.Property(i => i.LineTotal).HasColumnName("line_total").HasColumnType("numeric(10,2)");
    }
}

public class PrintSpoolConfiguration : IEntityTypeConfiguration<PrintSpool>
{
    public void Configure(EntityTypeBuilder<PrintSpool> builder)
    {
        builder.ToTable("print_spool");
        builder.HasKey(p => p.JobId);
        builder.Property(p => p.JobId).HasColumnName("job_id");
        builder.Property(p => p.OrderId).HasColumnName("order_id");
        builder.Property(p => p.JobType).HasColumnName("job_type").HasMaxLength(20).HasDefaultValue("KOT");
        builder.Property(p => p.RawContent).HasColumnName("raw_content").IsRequired();
        builder.Property(p => p.PrintStatus).HasColumnName("print_status").HasMaxLength(20).HasDefaultValue("PENDING");
        builder.Property(p => p.RetryCount).HasColumnName("retry_count").HasDefaultValue(0);
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.PrintedAt).HasColumnName("printed_at");
    }
}
