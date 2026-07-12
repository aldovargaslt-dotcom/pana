using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pana.Api.Domain.Operations;

namespace Pana.Api.Infrastructure.Data.Configurations;

public class ProductionEventConfiguration : IEntityTypeConfiguration<ProductionEvent>
{
    public void Configure(EntityTypeBuilder<ProductionEvent> builder)
    {
        builder.ToTable("production_events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(e => e.DailyProductionId).HasColumnName("daily_production_id").IsRequired();
        builder.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(e => e.ProductName).HasColumnName("product_name").HasMaxLength(300).IsRequired();
        builder.Property(e => e.EventType).HasColumnName("event_type").HasMaxLength(20).IsRequired();
        builder.Property(e => e.Quantity).HasColumnName("quantity").HasPrecision(18, 4).IsRequired();
        builder.Property(e => e.Notes).HasColumnName("notes").HasMaxLength(500);
        builder.Property(e => e.RegisteredByUserId).HasColumnName("registered_by_user_id").IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(e => new { e.DailyProductionId, e.ProductId, e.EventType });
        builder.HasIndex(e => e.DailyProductionId);
        builder.HasIndex(e => e.CreatedAt);
    }
}
