using Microsoft.EntityFrameworkCore;
using Search.Domain;

namespace Search.Data;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options), IDataContext
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }
}
