using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pana.Api.Domain.Production;

namespace Pana.Api.Infrastructure.Data.Configurations;

public class RecipeIngredientConfiguration : IEntityTypeConfiguration<RecipeIngredient>
{
    public void Configure(EntityTypeBuilder<RecipeIngredient> builder)
    {
        builder.ToTable("recipe_ingredients");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(i => i.RecipeId).HasColumnName("recipe_id").IsRequired();
        builder.Property(i => i.MaterialId).HasColumnName("material_id").IsRequired();
        builder.Property(i => i.Qty).HasColumnName("qty").HasPrecision(18, 4);
        builder.Property(i => i.Unit).HasColumnName("unit").HasMaxLength(50).IsRequired();
        builder.Property(i => i.ComputedCost).HasColumnName("computed_cost").HasPrecision(18, 4);
        builder.Property(i => i.CreatedAt).HasColumnName("created_at");
        builder.Property(i => i.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(i => i.RecipeId);
        builder.HasIndex(i => i.MaterialId);
    }
}
