using CatalogManager.Domain;
using Microsoft.EntityFrameworkCore;

namespace CatalogManager.Data;

public interface IDataContext
{
    DbSet<Product> Products { get; }
    DbSet<CatalogPublication> CatalogPublications { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
