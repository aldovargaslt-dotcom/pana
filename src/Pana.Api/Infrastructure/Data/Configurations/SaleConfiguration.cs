using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pana.Api.Domain.Sales;

namespace Pana.Api.Infrastructure.Data.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("sales");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(s => s.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(s => s.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(s => s.TotalAmount).HasColumnName("total_amount").HasPrecision(18, 2);
        builder.Property(s => s.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(s => s.SoldByUserId).HasColumnName("sold_by_user_id");
        builder.Property(s => s.CreatedAt).HasColumnName("created_at");
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        builder.HasMany(s => s.Items)
            .WithOne()
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => s.CreatedAt);
    }
}
