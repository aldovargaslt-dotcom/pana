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

        // Pre-order / customer fields
        builder.Property(s => s.OrderType).HasColumnName("order_type").HasMaxLength(50).IsRequired();
        builder.Property(s => s.CustomerName).HasColumnName("customer_name").HasMaxLength(200);
        builder.Property(s => s.CustomerPhone).HasColumnName("customer_phone").HasMaxLength(50);
        builder.Property(s => s.ScheduledDate).HasColumnName("scheduled_date");
        builder.Property(s => s.DepositAmount).HasColumnName("deposit_amount").HasPrecision(18, 2);
        builder.Property(s => s.PaymentStatus).HasColumnName("payment_status").HasMaxLength(50).IsRequired();
        builder.Property(s => s.PaymentMethod).HasColumnName("payment_method").HasMaxLength(50);
        builder.Property(s => s.InternalNotes).HasColumnName("internal_notes").HasMaxLength(2000);

        // Ignore computed properties
        builder.Ignore(s => s.BalanceDue);
        builder.Ignore(s => s.IsPreOrder);

        builder.HasMany(s => s.Items)
            .WithOne()
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => s.CreatedAt);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.OrderType);
        builder.HasIndex(s => s.ScheduledDate);
        builder.HasIndex(s => s.PaymentStatus);
    }
}
