using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;
using Search.Domain;

namespace Search.Data.Mapping;

public class ProductMapping : IEntityTypeConfiguration<Product>
{
    public const string SearchVector = "SearchVector";

    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(p => p.Sku);

        builder.Property(p => p.Sku).HasColumnName("sku").HasMaxLength(50);
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(p => p.Category).HasColumnName("category").HasMaxLength(50).IsRequired();
        builder.Property(p => p.Description).HasColumnName("description").HasMaxLength(1000).IsRequired();
        builder.Property(p => p.Origin).HasColumnName("origin").HasMaxLength(100);
        builder.Property(p => p.Region).HasColumnName("region").HasMaxLength(200);
        builder.Property(p => p.Process).HasColumnName("process").HasMaxLength(50);
        builder.Property(p => p.Roast).HasColumnName("roast").HasMaxLength(50);
        builder.Property(p => p.TastingNotes).HasColumnName("tasting_notes").HasMaxLength(500);

        // A generated column (see 0001_CreateProducts.sql), so it's read-only here and only used in queries.
        builder.Property<NpgsqlTsVector>(SearchVector)
            .HasColumnName("search_vector")
            .ValueGeneratedOnAddOrUpdate();
    }
}
