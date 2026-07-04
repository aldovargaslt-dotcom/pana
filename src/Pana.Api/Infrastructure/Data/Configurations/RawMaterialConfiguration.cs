using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pana.Api.Domain.Production;

namespace Pana.Api.Infrastructure.Data.Configurations;

public class RawMaterialConfiguration : IEntityTypeConfiguration<RawMaterial>
{
    public void Configure(EntityTypeBuilder<RawMaterial> builder)
    {
        builder.ToTable("raw_materials");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(m => m.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(m => m.Name).HasColumnName("name").HasMaxLength(300).IsRequired();
        builder.Property(m => m.Category).HasColumnName("category").HasMaxLength(100).IsRequired();
        builder.Property(m => m.PurchaseUnit).HasColumnName("purchase_unit").HasMaxLength(50).IsRequired();
        builder.Property(m => m.PurchasePrice).HasColumnName("purchase_price").HasPrecision(18, 4);
        builder.Property(m => m.YieldPct).HasColumnName("yield_pct").HasPrecision(5, 2).HasDefaultValue(100);
        builder.Property(m => m.PresentationQty).HasColumnName("presentation_qty").HasPrecision(18, 4);
        builder.Property(m => m.BaseUnit).HasColumnName("base_unit").HasMaxLength(50).IsRequired();
        builder.Property(m => m.Supplier).HasColumnName("supplier").HasMaxLength(300);
        builder.Property(m => m.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(m => m.CreatedAt).HasColumnName("created_at");
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");

        // CostPerBaseUnit is computed, not stored
        builder.Ignore(m => m.CostPerBaseUnit);

        builder.HasIndex(m => new { m.TenantId, m.Name }).IsUnique();
        builder.HasIndex(m => m.Category);
    }
}
