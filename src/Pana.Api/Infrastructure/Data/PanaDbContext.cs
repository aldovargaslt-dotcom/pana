using Microsoft.EntityFrameworkCore;
using Pana.Api.Domain.Accounting;
using Pana.Api.Domain.Common;
using Pana.Api.Domain.Identity;
using Pana.Api.Domain.Inventory;
using Pana.Api.Domain.Operations;
using Pana.Api.Domain.Products;
using Pana.Api.Domain.Production;
using Pana.Api.Domain.Sales;

namespace Pana.Api.Infrastructure.Data;

/// <summary>
/// Central database context for the Pana platform.
/// Applies multi-tenant query filters automatically.
/// </summary>
public class PanaDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public PanaDbContext(DbContextOptions<PanaDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<StockLocation> StockLocations => Set<StockLocation>();
    public DbSet<ReorderRule> ReorderRules => Set<ReorderRule>();
    public DbSet<UnitOfMeasure> UnitOfMeasures => Set<UnitOfMeasure>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
    public DbSet<DailyContext> DailyContexts => Set<DailyContext>();
    public DbSet<WasteCategory> WasteCategories => Set<WasteCategory>();
    public DbSet<WasteSubcategory> WasteSubcategories => Set<WasteSubcategory>();
    public DbSet<RawMaterial> RawMaterials => Set<RawMaterial>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<DailyProduction> DailyProductions => Set<DailyProduction>();
    public DbSet<DailyProductionLine> DailyProductionLines => Set<DailyProductionLine>();
    public DbSet<ProductionEvent> ProductionEvents => Set<ProductionEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PanaDbContext).Assembly);

        // Global query filter: every TenantEntity is scoped to the current tenant
        modelBuilder.Entity<Product>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<User>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<Sale>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<InventoryMovement>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<StockLocation>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<ReorderRule>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<UnitOfMeasure>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<JournalEntry>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<DailyContext>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<WasteCategory>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<RawMaterial>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<Recipe>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<DailyProduction>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        // JournalEntry → JournalEntryLine relationship
        modelBuilder.Entity<JournalEntry>()
            .HasMany(j => j.Lines)
            .WithOne()
            .HasForeignKey(l => l.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Sale → SaleItem relationship
        modelBuilder.Entity<Sale>()
            .HasMany(s => s.Items)
            .WithOne()
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        // StockLocation self-referencing hierarchy
        modelBuilder.Entity<StockLocation>()
            .HasIndex(l => l.ParentLocationId);

        // Indexes for performance
        modelBuilder.Entity<InventoryMovement>()
            .HasIndex(m => m.ProductId);

        modelBuilder.Entity<InventoryMovement>()
            .HasIndex(m => m.SourceLocationId);

        modelBuilder.Entity<InventoryMovement>()
            .HasIndex(m => m.DestinationLocationId);

        modelBuilder.Entity<ReorderRule>()
            .HasIndex(r => new { r.ProductId, r.LocationId });

        modelBuilder.Entity<JournalEntry>()
            .HasIndex(j => j.SourceSaleId);

        modelBuilder.Entity<JournalEntryLine>()
            .HasIndex(l => l.Account);
    }

    public override int SaveChanges()
    {
        SetAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.GetType().GetProperty(nameof(BaseEntity.CreatedAt))!
                    .SetValue(entry.Entity, DateTime.UtcNow);
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.GetType().GetProperty(nameof(BaseEntity.UpdatedAt))!
                    .SetValue(entry.Entity, DateTime.UtcNow);
            }
        }
    }
}
