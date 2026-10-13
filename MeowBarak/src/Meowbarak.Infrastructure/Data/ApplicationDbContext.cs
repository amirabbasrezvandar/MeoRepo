using Meowbarak.Application.Interfaces;
using Meowbarak.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Meowbarak.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<VerificationCode> VerificationCodes => Set<VerificationCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
        });

        // VerificationCode
        modelBuilder.Entity<VerificationCode>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Purpose).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => new { e.UserId, e.Code, e.Purpose });

            entity.HasOne(e => e.User)
                .WithMany(u => u.VerificationCodes)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
