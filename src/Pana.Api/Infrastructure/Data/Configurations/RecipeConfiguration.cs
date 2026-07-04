using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pana.Api.Domain.Production;

namespace Pana.Api.Infrastructure.Data.Configurations;

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.ToTable("recipes");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(r => r.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(r => r.Name).HasColumnName("name").HasMaxLength(300).IsRequired();
        builder.Property(r => r.Yield).HasColumnName("yield").HasPrecision(18, 4);
        builder.Property(r => r.YieldUnit).HasColumnName("yield_unit").HasMaxLength(50).IsRequired();
        builder.Property(r => r.LaborCostPerUnit).HasColumnName("labor_cost_per_unit").HasPrecision(18, 4).HasDefaultValue(0);
        builder.Property(r => r.EnergyCost).HasColumnName("energy_cost").HasPrecision(18, 4).HasDefaultValue(0);
        builder.Property(r => r.OverheadPct).HasColumnName("overhead_pct").HasPrecision(5, 2).HasDefaultValue(0);
        builder.Property(r => r.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(r => r.ProductId).HasColumnName("product_id");
        builder.Property(r => r.CreatedAt).HasColumnName("created_at");
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at");

        // Computed properties — not stored
        builder.Ignore(r => r.RawMaterialCost);
        builder.Ignore(r => r.LaborCost);
        builder.Ignore(r => r.OverheadCost);
        builder.Ignore(r => r.TotalBatchCost);
        builder.Ignore(r => r.CostPerUnit);

        builder.HasMany(r => r.Ingredients)
            .WithOne()
            .HasForeignKey(i => i.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.TenantId, r.Name }).IsUnique();
        builder.HasIndex(r => r.ProductId);
    }
}
