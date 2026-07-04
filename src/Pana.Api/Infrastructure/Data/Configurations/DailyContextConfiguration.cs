using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pana.Api.Domain.Common;

namespace Pana.Api.Infrastructure.Data.Configurations;

public class DailyContextConfiguration : IEntityTypeConfiguration<DailyContext>
{
    public void Configure(EntityTypeBuilder<DailyContext> builder)
    {
        builder.ToTable("daily_contexts");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(d => d.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(d => d.Date).HasColumnName("date").IsRequired();
        builder.Property(d => d.Weather).HasColumnName("weather").HasMaxLength(50);
        builder.Property(d => d.DayType).HasColumnName("day_type").HasMaxLength(50).IsRequired();
        builder.Property(d => d.IsPayday).HasColumnName("is_payday").HasDefaultValue(false);
        builder.Property(d => d.HasLocalEvent).HasColumnName("has_local_event").HasDefaultValue(false);
        builder.Property(d => d.SchoolOut).HasColumnName("school_out").HasDefaultValue(false);
        builder.Property(d => d.LowStock).HasColumnName("low_stock").HasDefaultValue(false);
        builder.Property(d => d.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(d => d.ClosedAt).HasColumnName("closed_at");
        builder.Property(d => d.CreatedAt).HasColumnName("created_at");
        builder.Property(d => d.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(d => new { d.TenantId, d.Date }).IsUnique();
    }
}
