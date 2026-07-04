using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pana.Api.Domain.Operations;

namespace Pana.Api.Infrastructure.Data.Configurations;

public class DailyProductionLineConfiguration : IEntityTypeConfiguration<DailyProductionLine>
{
    public void Configure(EntityTypeBuilder<DailyProductionLine> builder)
    {
        builder.ToTable("daily_production_lines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(l => l.DailyProductionId).HasColumnName("daily_production_id").IsRequired();
        builder.Property(l => l.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(l => l.ProductName).HasColumnName("product_name").HasMaxLength(300).IsRequired();
        builder.Property(l => l.Inicial).HasColumnName("inicial").HasPrecision(18, 4).HasDefaultValue(0);
        builder.Property(l => l.Produccion).HasColumnName("produccion").HasPrecision(18, 4).HasDefaultValue(0);
        builder.Property(l => l.Devolucion).HasColumnName("devolucion").HasPrecision(18, 4).HasDefaultValue(0);
        builder.Property(l => l.CreatedAt).HasColumnName("created_at");
        builder.Property(l => l.UpdatedAt).HasColumnName("updated_at");

        // Computed columns
        builder.Ignore(l => l.Disponible);

        builder.HasIndex(l => new { l.DailyProductionId, l.ProductId }).IsUnique();
        builder.HasIndex(l => l.ProductId);
    }
}
