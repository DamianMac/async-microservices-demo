using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain;

namespace Orders.Data.Mapping;

public class OrderMapping : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();
        builder.HasIndex(o => o.CustomerId);

        builder.Property(o => o.CustomerEmail).HasMaxLength(256).IsRequired();
        builder.Property(o => o.CustomerFirstName).HasMaxLength(100).IsRequired();
        builder.Property(o => o.CustomerLastName).HasMaxLength(100).IsRequired();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.OwnsOne(o => o.DeliveryAddress, a =>
        {
            a.Property(x => x.RecipientName).HasColumnName("DeliveryRecipientName").HasMaxLength(200).IsRequired();
            a.Property(x => x.Line1).HasColumnName("DeliveryLine1").HasMaxLength(200).IsRequired();
            a.Property(x => x.Line2).HasColumnName("DeliveryLine2").HasMaxLength(200);
            a.Property(x => x.Suburb).HasColumnName("DeliverySuburb").HasMaxLength(100).IsRequired();
            a.Property(x => x.State).HasColumnName("DeliveryState").HasMaxLength(50).IsRequired();
            a.Property(x => x.Postcode).HasColumnName("DeliveryPostcode").HasMaxLength(10).IsRequired();
            a.Property(x => x.Country).HasColumnName("DeliveryCountry").HasMaxLength(100).IsRequired();
            a.Property(x => x.Phone).HasColumnName("DeliveryPhone").HasMaxLength(30);
        });
        builder.Navigation(o => o.DeliveryAddress).IsRequired();

        builder.Property(o => o.CardBrand).HasMaxLength(20).IsRequired();
        builder.Property(o => o.CardLast4).HasMaxLength(4).IsFixedLength().IsRequired();

        builder.Property(o => o.Subtotal).HasPrecision(10, 2);
        builder.Property(o => o.Shipping).HasPrecision(10, 2);
        builder.Property(o => o.Total).HasPrecision(10, 2);
        builder.Property(o => o.Currency).HasMaxLength(3).IsFixedLength().IsRequired();

        builder.Property(o => o.PaymentTransactionId).HasMaxLength(100);

        // Times are stored as UTC; mark them as such when read, since datetime2 doesn't keep the kind.
        builder.Property(o => o.PlacedAt).HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        builder.Property(o => o.UpdatedAt).HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        builder.Property(o => o.PaidAt).HasConversion(
            v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);
        builder.Property(o => o.CompletedAt).HasConversion(
            v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        builder.HasMany(o => o.Lines).WithOne().HasForeignKey(l => l.OrderId).OnDelete(DeleteBehavior.Cascade);
    }
}
