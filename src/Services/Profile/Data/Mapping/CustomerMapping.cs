using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profile.Domain;

namespace Profile.Data.Mapping;

public class CustomerMapping : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.Email).IsUnique();

        builder.Property(c => c.Email).HasMaxLength(256).IsRequired();
        builder.Property(c => c.PasswordHash).HasMaxLength(200).IsRequired();
        builder.Property(c => c.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.LastName).HasMaxLength(100).IsRequired();

        // Times are stored as UTC; mark them as such when read, since datetime2 doesn't keep the kind.
        builder.Property(c => c.CreatedAt).HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        builder.Property(c => c.UpdatedAt).HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        builder.HasMany(c => c.Addresses).WithOne().HasForeignKey(a => a.CustomerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.Cards).WithOne().HasForeignKey(c => c.CustomerId).OnDelete(DeleteBehavior.Cascade);
    }
}
