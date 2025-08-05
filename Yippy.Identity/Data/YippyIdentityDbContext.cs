using Microsoft.EntityFrameworkCore;

namespace Yippy.Identity.Data;

public class YippyIdentityDbContext(DbContextOptions<YippyIdentityDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<UserAccess> UserAccesses => Set<UserAccess>();
    
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
    }
}