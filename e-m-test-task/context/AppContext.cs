using Microsoft.EntityFrameworkCore;

public class AppContext : DbContext
{
    public DbSet<District> Districts => Set<District>();
    public DbSet<Order> Orders => Set<Order>();
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=OrdersAndDisctricts;Username=postgres;Password=1563");
        
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<District>()
        .HasKey(b => b.DistrictId)
        .HasName("PrimaryKey_DisctrictId");
    }
}