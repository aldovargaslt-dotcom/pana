using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pana.Api.Domain.Sales;

namespace Pana.Api.Infrastructure.Data.Configurations;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("sale_items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(i => i.SaleId).HasColumnName("sale_id").IsRequired();
        builder.Property(i => i.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(i => i.ProductName).HasColumnName("product_name").HasMaxLength(300).IsRequired();
        builder.Property(i => i.UnitPrice).HasColumnName("unit_price").HasPrecision(18, 2);
        builder.Property(i => i.Quantity).HasColumnName("quantity");
        builder.Property(i => i.IsVoided).HasColumnName("is_voided").HasDefaultValue(false);
        builder.Property(i => i.CreatedAt).HasColumnName("created_at");
        builder.Property(i => i.UpdatedAt).HasColumnName("updated_at");

        builder.Ignore(i => i.LineTotal);

        builder.HasIndex(i => i.SaleId);
        builder.HasIndex(i => i.ProductId);
    }
}
