using IdentityData.Models;

namespace IdentityData.Data;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<ProcessedEventEntity> ProcessedEvents { get; set; } = default!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasKey(u => u.KeycloakId);
            entity.HasIndex(u => u.Username).HasDatabaseName("IX_User_Username");
            entity.HasIndex(u => u.Email).HasDatabaseName("IX_User_Email");
            entity.HasIndex(u => u.RealmId);

            entity.Property(u => u.EmailVerified).HasDefaultValue(false);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
        modelBuilder.Entity<ProcessedEventEntity>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasIndex(e => e.Uid).IsUnique();
        });
    }
}
