using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profile.Domain;

namespace Profile.Data.Mapping;

public class PaymentCardMapping : IEntityTypeConfiguration<PaymentCard>
{
    public void Configure(EntityTypeBuilder<PaymentCard> builder)
    {
        builder.ToTable("PaymentCards");
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.CustomerId);

        builder.Property(c => c.CardholderName).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Brand).HasMaxLength(20).IsRequired();
        builder.Property(c => c.Last4).HasMaxLength(4).IsFixedLength().IsRequired();

        builder.Property(c => c.CreatedAt).HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
    }
}
