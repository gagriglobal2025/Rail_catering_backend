using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RailCat.Catalog.Api.Models.Entities;

namespace RailCat.Catalog.Api.Data.Configurations;

public class MenuCategoryConfiguration : IEntityTypeConfiguration<MenuCategory>
{
    public void Configure(EntityTypeBuilder<MenuCategory> builder)
    {
        builder.ToTable("menu_categories");
        builder.HasKey(c => c.CategoryId);
        builder.Property(c => c.CategoryId).HasColumnName("category_id").HasMaxLength(50);
        builder.Property(c => c.CategoryName).HasColumnName("category_name").HasMaxLength(100).IsRequired();
        builder.Property(c => c.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
        builder.Property(c => c.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");

        builder.HasMany(c => c.Items)
               .WithOne(i => i.Category)
               .HasForeignKey(i => i.CategoryId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("menu_items");
        builder.HasKey(i => i.ItemId);
        builder.Property(i => i.ItemId).HasColumnName("item_id").HasMaxLength(50);
        builder.Property(i => i.CategoryId).HasColumnName("category_id").HasMaxLength(50).IsRequired();
        builder.Property(i => i.ItemName).HasColumnName("item_name").HasMaxLength(100).IsRequired();
        builder.Property(i => i.ItemDescription).HasColumnName("item_description").IsRequired();
        builder.Property(i => i.BasePrice).HasColumnName("base_price").HasColumnType("numeric(10,2)");
        builder.Property(i => i.EmojiIcon).HasColumnName("emoji_icon").HasMaxLength(10);
        builder.Property(i => i.ImageUrl).HasColumnName("image_url").HasMaxLength(500);
        builder.Property(i => i.IsVegetarian).HasColumnName("is_vegetarian");
        builder.Property(i => i.IsAvailable).HasColumnName("is_available").HasDefaultValue(true);
        builder.Property(i => i.IsSoldOut).HasColumnName("is_sold_out").HasDefaultValue(false);
        builder.Property(i => i.IsBestseller).HasColumnName("is_bestseller").HasDefaultValue(false);
        builder.Property(i => i.IsSpicy).HasColumnName("is_spicy").HasDefaultValue(false);
        builder.Property(i => i.IsKidsChoice).HasColumnName("is_kids_choice").HasDefaultValue(false);
        builder.Property(i => i.Rating).HasColumnName("rating").HasColumnType("numeric(2,1)");
        builder.Property(i => i.RatingCount).HasColumnName("rating_count").HasDefaultValue(0);
        builder.Property(i => i.PrepTime).HasColumnName("prep_time").HasMaxLength(20);
        builder.Property(i => i.CreatedAt).HasColumnName("created_at");
        builder.Property(i => i.UpdatedAt).HasColumnName("updated_at");
    }
}

public class ConcessionPolicyConfiguration : IEntityTypeConfiguration<ConcessionPolicy>
{
    public void Configure(EntityTypeBuilder<ConcessionPolicy> builder)
    {
        builder.ToTable("concession_policies");
        builder.HasKey(p => p.PolicyId);
        builder.Property(p => p.PolicyId).HasColumnName("policy_id").HasMaxLength(50);
        builder.Property(p => p.PolicyName).HasColumnName("policy_name").HasMaxLength(100).IsRequired();
        builder.Property(p => p.CustomerType).HasColumnName("customer_type").HasMaxLength(30).IsRequired();
        builder.Property(p => p.DiscountPercentage).HasColumnName("discount_percentage").HasColumnType("numeric(5,2)").HasDefaultValue(20.00m);
        builder.Property(p => p.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
    }
}

public class RailwayConcessionEmployeeConfiguration : IEntityTypeConfiguration<RailwayConcessionEmployee>
{
    public void Configure(EntityTypeBuilder<RailwayConcessionEmployee> builder)
    {
        builder.ToTable("railway_concession_employees");
        builder.HasKey(e => e.EmployeeId);
        builder.Property(e => e.EmployeeId).HasColumnName("employee_id").HasMaxLength(30);
        builder.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Email).HasColumnName("email").HasMaxLength(120);
        builder.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
        builder.Property(e => e.Department).HasColumnName("department").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Designation).HasColumnName("designation").HasMaxLength(100).IsRequired();
        builder.Property(e => e.ShiftPolicy).HasColumnName("shift_policy").HasMaxLength(50);
        builder.Property(e => e.VerificationStatus).HasColumnName("verification_status").HasMaxLength(20).IsRequired();
        builder.Property(e => e.DeviceName).HasColumnName("device_name").HasMaxLength(100);
        builder.Property(e => e.DeviceToken).HasColumnName("device_token").HasMaxLength(100);
        builder.Property(e => e.DeviceLocked).HasColumnName("device_locked").HasDefaultValue(false);
        builder.Property(e => e.IdCardUrl).HasColumnName("id_card_url");
        builder.Property(e => e.RejectionReason).HasColumnName("rejection_reason");
        builder.Property(e => e.RegisteredAt).HasColumnName("registered_at");
        builder.Property(e => e.VerifiedAt).HasColumnName("verified_at");
    }
}

