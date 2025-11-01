namespace Catalog.Data;

public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> Categories => Set<ProductCategory>();
    public DbSet<ProductTag> Tags => Set<ProductTag>();
    public DbSet<ProductImage> Images => Set<ProductImage>();
    public DbSet<ProductVariant> Variants => Set<ProductVariant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name).IsRequired();
            entity.Property(p => p.Description).IsRequired();
            entity.Property(p => p.Brand).IsRequired();
            entity.Property(p => p.BasePrice).HasColumnType("numeric(18,2)");
            entity.Property(p => p.CreatedAt).HasColumnType("timestamp with time zone");

            entity.HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(p => p.Images)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Variants)
                .WithOne(v => v.Product)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Tags)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "ProductTagAssignments",
                    j => j.HasOne<ProductTag>()
                        .WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j.HasOne<Product>()
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("ProductId", "TagId");
                        j.ToTable("ProductTagLinks");
                    });
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.Property(c => c.Name).IsRequired();
            entity.HasOne(c => c.ParentCategory)
                .WithMany()
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductTag>(entity =>
        {
            entity.Property(t => t.Name).IsRequired();
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.Property(i => i.Url).IsRequired();
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.Property(v => v.Sku).IsRequired();
            entity.Property(v => v.Price).HasColumnType("numeric(18,2)");
        });
    }
}
