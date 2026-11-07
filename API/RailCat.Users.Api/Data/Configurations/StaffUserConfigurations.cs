using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RailCat.Users.Api.Models.Entities;

namespace RailCat.Users.Api.Data.Configurations;

public class StaffUserConfiguration : IEntityTypeConfiguration<StaffUser>
{
    public void Configure(EntityTypeBuilder<StaffUser> builder)
    {
        builder.ToTable("staff_users");

        builder.HasKey(u => u.UserId);
        builder.Property(u => u.UserId).HasColumnName("user_id");
        builder.Property(u => u.EmployeeId).HasColumnName("employee_id").HasMaxLength(30).IsRequired();
        builder.HasIndex(u => u.EmployeeId).IsUnique();

        builder.Property(u => u.FullName).HasColumnName("full_name").HasMaxLength(100).IsRequired();
        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(120).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PhoneNumber).HasColumnName("phone_number").HasMaxLength(20);
        builder.Property(u => u.SystemRole).HasColumnName("system_role").HasMaxLength(30).IsRequired();
        builder.Property(u => u.AccountStatus).HasColumnName("account_status").HasMaxLength(20).IsRequired();
        builder.Property(u => u.LastActiveAt).HasColumnName("last_active_at");
        builder.Property(u => u.CreatedAt).HasColumnName("created_at");
        builder.Property(u => u.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(u => u.Credential)
               .WithOne(c => c.User)
               .HasForeignKey<StaffCredential>(c => c.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Sessions)
               .WithOne(s => s.User)
               .HasForeignKey(s => s.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StaffCredentialConfiguration : IEntityTypeConfiguration<StaffCredential>
{
    public void Configure(EntityTypeBuilder<StaffCredential> builder)
    {
        builder.ToTable("staff_credentials");

        builder.HasKey(c => c.UserId);
        builder.Property(c => c.UserId).HasColumnName("user_id");
        builder.Property(c => c.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
        builder.Property(c => c.FailedLogins).HasColumnName("failed_logins").HasDefaultValue(0);
        builder.Property(c => c.LockedUntil).HasColumnName("locked_until");
        builder.Property(c => c.PasswordUpdatedAt).HasColumnName("password_updated_at");
    }
}

public class StaffSessionConfiguration : IEntityTypeConfiguration<StaffSession>
{
    public void Configure(EntityTypeBuilder<StaffSession> builder)
    {
        builder.ToTable("staff_sessions");

        builder.HasKey(s => s.SessionId);
        builder.Property(s => s.SessionId).HasColumnName("session_id");
        builder.Property(s => s.UserId).HasColumnName("user_id");
        builder.Property(s => s.TokenHash).HasColumnName("token_hash").HasMaxLength(255).IsRequired();
        builder.Property(s => s.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
        builder.Property(s => s.UserAgent).HasColumnName("user_agent").HasMaxLength(255);
        builder.Property(s => s.ExpiresAt).HasColumnName("expires_at");
        builder.Property(s => s.CreatedAt).HasColumnName("created_at");
    }
}
