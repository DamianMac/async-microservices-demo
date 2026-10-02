using CatalogManager.Domain;
using Microsoft.EntityFrameworkCore;

namespace CatalogManager.Data;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options), IDataContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<CatalogPublication> CatalogPublications => Set<CatalogPublication>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }
}
