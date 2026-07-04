using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pana.Api.Domain.Inventory;

namespace Pana.Api.Infrastructure.Data.Configurations;

public class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.ToTable("inventory_movements");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(m => m.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(m => m.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(m => m.MovementType).HasColumnName("movement_type").HasMaxLength(50).IsRequired();
        builder.Property(m => m.Quantity).HasColumnName("quantity").HasPrecision(18, 4);
        builder.Property(m => m.Reason).HasColumnName("reason").HasMaxLength(500);
        builder.Property(m => m.ReferenceSaleId).HasColumnName("reference_sale_id");
        builder.Property(m => m.PerformedByUserId).HasColumnName("performed_by_user_id");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at");
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(m => new { m.TenantId, m.ProductId });
        builder.HasIndex(m => m.CreatedAt);
    }
}
