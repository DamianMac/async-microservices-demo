using Microsoft.EntityFrameworkCore;
using Profile.Domain;

namespace Profile.Data;

public interface IDataContext
{
    DbSet<Customer> Customers { get; }
    DbSet<DeliveryAddress> DeliveryAddresses { get; }
    DbSet<PaymentCard> PaymentCards { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
