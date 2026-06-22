using BakeryPay.Backend.Interfaces.Repositories;
using BakeryPay.Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace BakeryPay.Backend.Data;

public class BakeryPayDbContext : DbContext, IUnitOfWork
{
    public BakeryPayDbContext(DbContextOptions<BakeryPayDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<UserBiometricCredential> UserBiometricCredentials => Set<UserBiometricCredential>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(200).IsRequired();
            entity.Property(x => x.IsSystemRole).HasDefaultValue(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.IdentificationNumber).IsUnique();
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.IdentificationNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(150).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(30);
            entity.Property(x => x.HasAcceptedPolicies).HasDefaultValue(false);
            entity.Property(x => x.MustChangePassword).HasDefaultValue(false);

            entity.HasOne(x => x.Role)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Provider)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Provider>(entity =>
        {
            entity.ToTable("Providers");
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.TaxId).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(30).IsRequired();
            entity.Property(x => x.BusinessName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.TaxId).HasMaxLength(20).IsRequired();
            entity.Property(x => x.ContactName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ContactIdentificationNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(30);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Currency).HasMaxLength(5).IsRequired();
            entity.Property(x => x.ReferenceNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);

            entity.HasOne(x => x.Provider)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RegisteredByUser)
                .WithMany(x => x.RegisteredPayments)
                .HasForeignKey(x => x.RegisteredByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Receipt>(entity =>
        {
            entity.ToTable("Receipts");
            entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.StoredFileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.StoragePath).HasMaxLength(400).IsRequired();

            entity.HasOne(x => x.Payment)
                .WithMany(x => x.Receipts)
                .HasForeignKey(x => x.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(500).IsRequired();

            entity.HasOne(x => x.Provider)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserBiometricCredential>(entity =>
        {
            entity.ToTable("UserBiometricCredentials");
            entity.HasIndex(x => new { x.UserId, x.DeviceId }).IsUnique();
            entity.Property(x => x.DeviceId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.DeviceName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Platform).HasMaxLength(40).IsRequired();

            entity.HasOne(x => x.User)
                .WithMany(x => x.BiometricCredentials)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
