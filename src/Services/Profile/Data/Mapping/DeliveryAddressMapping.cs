using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profile.Domain;

namespace Profile.Data.Mapping;

public class DeliveryAddressMapping : IEntityTypeConfiguration<DeliveryAddress>
{
    public void Configure(EntityTypeBuilder<DeliveryAddress> builder)
    {
        builder.ToTable("DeliveryAddresses");
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => a.CustomerId);

        builder.Property(a => a.Label).HasMaxLength(50).IsRequired();
        builder.Property(a => a.RecipientName).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Line1).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Line2).HasMaxLength(200);
        builder.Property(a => a.Suburb).HasMaxLength(100).IsRequired();
        builder.Property(a => a.State).HasMaxLength(50).IsRequired();
        builder.Property(a => a.Postcode).HasMaxLength(10).IsRequired();
        builder.Property(a => a.Country).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Phone).HasMaxLength(30);

        builder.Property(a => a.CreatedAt).HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        builder.Property(a => a.UpdatedAt).HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
    }
}
