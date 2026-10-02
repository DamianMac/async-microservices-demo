using Microsoft.EntityFrameworkCore;
using Profile.Domain;

namespace Profile.Data;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options), IDataContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<DeliveryAddress> DeliveryAddresses => Set<DeliveryAddress>();
    public DbSet<PaymentCard> PaymentCards => Set<PaymentCard>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }
}
