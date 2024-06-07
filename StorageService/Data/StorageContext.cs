using Microsoft.EntityFrameworkCore;

namespace StorageService.Data;

public class StorageContext : DbContext
{
    public DbSet<StorageItem> Items { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite("Data Source=Storage.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StorageItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("items");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Extension).HasColumnName("extension").IsRequired();
            entity.Property(e => e.ModifiedDate).HasColumnName("modified_date").IsRequired();
            entity.Property(e => e.OriginalPath).HasColumnName("original_path").IsRequired();
            entity.Property(e => e.OriginalSource).HasColumnName("original_source").IsRequired();
            entity.Property(e => e.UploadedDate).HasColumnName("uploaded_date").IsRequired();
        });
    }
}