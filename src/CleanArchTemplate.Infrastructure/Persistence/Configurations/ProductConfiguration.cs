using CleanArchTemplate.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchTemplate.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("products");
        builder.HasKey(product => product.Id);

        builder.Property(product => product.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new ProductId(value))
            .ValueGeneratedNever();

        builder.Property(product => product.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.OwnsOne(product => product.Sku, owned =>
            owned.Property(sku => sku.Value)
                .HasColumnName("sku")
                .HasMaxLength(50)
                .IsRequired());

        builder.OwnsOne(product => product.Price, owned =>
        {
            owned.Property(money => money.Amount)
                .HasColumnName("price_amount")
                .HasPrecision(18, 2);
            owned.Property(money => money.Currency)
                .HasColumnName("price_currency")
                .HasMaxLength(3);
        });
    }
}
