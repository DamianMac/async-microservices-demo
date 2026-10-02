using CatalogManager.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogManager.Data.Mapping;

public class ProductMapping : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Sku).IsUnique();

        builder.Property(p => p.Sku).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Category).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(1000).IsRequired();
        builder.Property(p => p.ImageUrl).HasMaxLength(300);
        builder.Property(p => p.Price).HasPrecision(10, 2);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

        // Times are stored as UTC; mark them as such when read, since datetime2 doesn't keep the kind.
        builder.Property(p => p.CreatedAt).HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        builder.Property(p => p.UpdatedAt).HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        builder.Property(p => p.PublishedAt).HasConversion(v => v, v => v == null ? null : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));

        builder.OwnsOne(p => p.Coffee, coffee =>
        {
            coffee.Property(c => c.Origin).HasColumnName("CoffeeOrigin").HasMaxLength(100);
            coffee.Property(c => c.Region).HasColumnName("CoffeeRegion").HasMaxLength(200);
            coffee.Property(c => c.Process).HasColumnName("CoffeeProcess").HasMaxLength(50);
            coffee.Property(c => c.Roast).HasColumnName("CoffeeRoast").HasMaxLength(50);
            coffee.Property(c => c.WeightGrams).HasColumnName("CoffeeWeightGrams");

            // Stored as a comma-separated list; the notes are single words or short phrases.
            coffee.Property(c => c.TastingNotes)
                .HasColumnName("CoffeeTastingNotes")
                .HasMaxLength(500)
                .HasConversion(
                    notes => string.Join(",", notes),
                    value => value.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    new ValueComparer<List<string>>(
                        (a, b) => a!.SequenceEqual(b!),
                        notes => notes.Aggregate(0, (hash, note) => HashCode.Combine(hash, note.GetHashCode())),
                        notes => notes.ToList()));
        });
    }
}
