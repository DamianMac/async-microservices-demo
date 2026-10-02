using CatalogManager.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogManager.Data.Mapping;

public class CatalogPublicationMapping : IEntityTypeConfiguration<CatalogPublication>
{
    public void Configure(EntityTypeBuilder<CatalogPublication> builder)
    {
        builder.ToTable("CatalogPublications");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PublishedAt).HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
    }
}
