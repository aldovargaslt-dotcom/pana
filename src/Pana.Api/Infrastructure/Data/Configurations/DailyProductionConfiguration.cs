using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pana.Api.Domain.Operations;

namespace Pana.Api.Infrastructure.Data.Configurations;

public class DailyProductionConfiguration : IEntityTypeConfiguration<DailyProduction>
{
    public void Configure(EntityTypeBuilder<DailyProduction> builder)
    {
        builder.ToTable("daily_productions");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(d => d.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(d => d.DailyContextId).HasColumnName("daily_context_id").IsRequired();
        builder.Property(d => d.Date).HasColumnName("date").IsRequired();
        builder.Property(d => d.IsClosed).HasColumnName("is_closed").HasDefaultValue(false);
        builder.Property(d => d.ClosedAt).HasColumnName("closed_at");
        builder.Property(d => d.ClosedByUserId).HasColumnName("closed_by_user_id");
        builder.Property(d => d.CreatedAt).HasColumnName("created_at");
        builder.Property(d => d.UpdatedAt).HasColumnName("updated_at");

        builder.HasMany(d => d.Lines)
            .WithOne()
            .HasForeignKey(l => l.DailyProductionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => new { d.TenantId, d.Date }).IsUnique();
        builder.HasIndex(d => d.DailyContextId);
    }
}
