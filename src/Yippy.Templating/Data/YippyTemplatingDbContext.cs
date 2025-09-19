using Microsoft.EntityFrameworkCore;

namespace Yippy.Templating.Data;

public class YippyTemplatingDbContext(DbContextOptions<YippyTemplatingDbContext> options) : DbContext(options)
{
    public DbSet<EmailTemplate> EmailTemplates { get; set; }
    
    public DbSet<SmsTemplate> SmsTemplates { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Email Template Configuration
        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.FromName)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.FromEmail)
                .IsRequired()
                .HasMaxLength(320)
                .IsUnicode(false);
            
            entity.Property(e => e.Subject)
                .IsRequired()
                .HasMaxLength(500);
            
            entity.Property(e => e.Body)
                .IsRequired()
                .HasColumnType("text");
            
            // Indexes
            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("UX_EmailTemplates_TemplateName");
        });
        
        // SMS Template Configuration
        modelBuilder.Entity<SmsTemplate>(entity =>
        {
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.FromName)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Body)
                .IsRequired()
                .HasMaxLength(1600);

            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("UX_SmsTemplates_TemplateName");
        });
    }
}