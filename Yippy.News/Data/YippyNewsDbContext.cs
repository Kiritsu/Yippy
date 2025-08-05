using Microsoft.EntityFrameworkCore;

namespace Yippy.News.Data;

public class YippyNewsDbContext(DbContextOptions<YippyNewsDbContext> options) : DbContext(options)
{
    public DbSet<Post> Posts => Set<Post>();
    
    public DbSet<PostRevision> PostRevisions => Set<PostRevision>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>()
            .Property(e => e.Title)
            .HasMaxLength(128);

        modelBuilder.Entity<Post>()
            .Property(e => e.Body)
            .HasMaxLength(65536);
        
        modelBuilder.Entity<Post>()
            .HasMany(x => x.Revisions)
            .WithOne()
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}