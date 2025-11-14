using Domain.Entities;
using Domain.Vo;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // RefreshTokenEntity
        modelBuilder.Entity<RefreshTokenEntity>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.Property(rt => rt.Token).IsRequired();
            entity.Property(rt => rt.ExpiryDate).IsRequired();
            entity.Property(rt => rt.IsRevoked).IsRequired();
        });

        // UserEntity
        modelBuilder.Entity<UserEntity>(builder =>
        {
            builder.Property(u => u.Email)
                .HasConversion(
                    e => e.Value,
                    value => new EmailAddress(value)
                )
                .HasMaxLength(254)
                .IsRequired();

            builder.Property(u => u.PhoneNumber)
                .HasConversion(
                    p => p.Value,
                    value => new PhoneNumber(value)
                )
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(u => u.Role)
                .HasConversion(
                    r => r.Value,
                    value => new Role(value)
                )
                .HasMaxLength(50)
                .IsRequired();

            //Address (VO complexe)
            builder.OwnsOne(u => u.Address, addressBuilder =>
            {
                addressBuilder.Property(a => a.Street)
                    .HasMaxLength(100)
                    .IsRequired();

                addressBuilder.Property(a => a.City)
                    .HasMaxLength(50)
                    .IsRequired();

                addressBuilder.Property(a => a.PostalCode)
                    .HasMaxLength(20)
                    .IsRequired();

                addressBuilder.Property(a => a.Country)
                    .HasMaxLength(50)
                    .IsRequired();
            });

            //Password
            builder.Property(u => u.Password)
                .HasConversion(
                    p => p.HashedValue,
                    value => new Password(value)
                )
                .HasMaxLength(200)
                .IsRequired();


            //Indexes  Perf
            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.PhoneNumber).IsUnique();
            builder.HasIndex(u => u.Role);
            builder.HasIndex(u => u.LastLoginDate);
        });
    }

}