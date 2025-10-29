using Microsoft.EntityFrameworkCore;

namespace Yippy.Identity.Data;

public class YippyIdentityDbContext(DbContextOptions<YippyIdentityDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<UserAccess> UserAccesses => Set<UserAccess>();

    public DbSet<JwtGuard> JwtGuards => Set<JwtGuard>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(e => e.Email)
            .HasMaxLength(128);
            
        modelBuilder.Entity<UserAccess>()
            .HasOne(e => e.User)
            .WithMany(e => e.UserAccesses)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<JwtGuard>()
            .Property(e => e.TokenHashSha256)
            .HasMaxLength(64);

        modelBuilder.Entity<JwtGuard>()
            .HasIndex(e => e.TokenHashSha256)
            .IsUnique();

        modelBuilder.Entity<JwtGuard>()
            .HasIndex(e => e.TokenExpiresAtUtc);
    }
}