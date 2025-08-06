using Microsoft.EntityFrameworkCore;

namespace Yippy.News.Data;

public class YippyNewsDbContext(DbContextOptions<YippyNewsDbContext> options) : DbContext(options)
{
    public DbSet<Post> Posts => Set<Post>();
    
    public DbSet<PostRevision> PostRevisions => Set<PostRevision>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(post =>
        {
            post
                .Property(e => e.Title)
                .HasMaxLength(128);

            post
                .Property(e => e.Body)
                .HasMaxLength(65536);
        
            post
                .HasMany(x => x.Revisions)
                .WithOne()
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}