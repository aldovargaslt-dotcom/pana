using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pana.Api.Domain.Inventory;

namespace Pana.Api.Infrastructure.Data.Configurations;

public class WasteCategoryConfiguration : IEntityTypeConfiguration<WasteCategory>
{
    public void Configure(EntityTypeBuilder<WasteCategory> builder)
    {
        builder.ToTable("waste_categories");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(w => w.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(w => w.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(w => w.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(w => w.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
        builder.Property(w => w.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(w => w.CreatedAt).HasColumnName("created_at");
        builder.Property(w => w.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(w => new { w.TenantId, w.Name }).IsUnique();

        builder.HasMany(w => w.Subcategories)
            .WithOne()
            .HasForeignKey(s => s.WasteCategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
