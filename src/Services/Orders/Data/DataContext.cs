using Microsoft.EntityFrameworkCore;
using Orders.Domain;

namespace Orders.Data;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options), IDataContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }
}
