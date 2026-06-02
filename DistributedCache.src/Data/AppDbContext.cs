using DistributedCache.Models;
using DistributedCache.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DistributedCache.Data;

public class AppDbContext : DbContext
{
   private DatabaseSettings _dbSettings;
    public AppDbContext(DbContextOptions<AppDbContext> options,IOptions<DatabaseSettings> dbSettings) : base(options)
    {
        this._dbSettings = dbSettings.Value;
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql(this._dbSettings.StringConnection);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop", Description = "Laptop gaming 16GB RAM", Price = 1299.99m, Stock = 15 },
            new Product { Id = 2, Name = "Mouse", Description = "Mouse inalambrico ergonomico", Price = 29.99m, Stock = 200 },
            new Product { Id = 3, Name = "Teclado", Description = "Teclado mecanico RGB", Price = 89.99m, Stock = 75 },
            new Product { Id = 4, Name = "Monitor", Description = "Monitor 27 pulgadas 4K", Price = 449.99m, Stock = 30 },
            new Product { Id = 5, Name = "Auriculares", Description = "Auriculares bluetooth con cancelacion de ruido", Price = 199.99m, Stock = 50 }
        );
    }
}
