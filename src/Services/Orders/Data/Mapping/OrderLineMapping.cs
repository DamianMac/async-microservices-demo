using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain;

namespace Orders.Data.Mapping;

public class OrderLineMapping : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("OrderLines");
        builder.HasKey(l => l.Id);
        builder.HasIndex(l => l.OrderId);

        builder.Property(l => l.Sku).HasMaxLength(50).IsRequired();
        builder.Property(l => l.Name).HasMaxLength(200).IsRequired();
        builder.Property(l => l.UnitPrice).HasPrecision(10, 2);
        builder.Property(l => l.LineTotal).HasPrecision(10, 2);
    }
}
