using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pana.Api.Domain.Inventory;

namespace Pana.Api.Infrastructure.Data.Configurations;

public class WasteSubcategoryConfiguration : IEntityTypeConfiguration<WasteSubcategory>
{
    public void Configure(EntityTypeBuilder<WasteSubcategory> builder)
    {
        builder.ToTable("waste_subcategories");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(s => s.WasteCategoryId).HasColumnName("waste_category_id").IsRequired();
        builder.Property(s => s.Name).HasColumnName("name").HasMaxLength(200).IsRequired();

        builder.HasIndex(s => new { s.WasteCategoryId, s.Name }).IsUnique();
    }
}
