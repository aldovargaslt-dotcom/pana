using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pana.Api.Application.Accounting;
using Pana.Api.Application.Analytics;
using Pana.Api.Application.Common;
using Pana.Api.Application.Export;
using Pana.Api.Application.Identity;
using Pana.Api.Application.Inventory;
using Pana.Api.Application.Operations;
using Pana.Api.Application.Products;
using Pana.Api.Application.Production;
using Pana.Api.Application.Sales;
using Pana.Api.Domain.Common;
using Pana.Api.Domain.Sales;
using Pana.Api.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Logging ────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"));

// ── Database ───────────────────────────────────────────────────
builder.Services.AddDbContext<PanaDbContext>((sp, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly(typeof(PanaDbContext).Assembly.FullName);
    });
});

// ── JWT Authentication ─────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? Environment.GetEnvironmentVariable("JWT__KEY")
    ?? throw new InvalidOperationException("JWT key is not configured. Set Jwt:Key in appsettings or JWT__KEY env var.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("StaffOrAbove", policy => policy.RequireRole("Admin", "Manager", "Staff"));
});

// ── Tenant Context ─────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();

// ── Application Services ───────────────────────────────────────
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IStockLocationService, StockLocationService>();
builder.Services.AddScoped<IReorderRuleService, ReorderRuleService>();
builder.Services.AddScoped<IAccountingService, AccountingService>();
builder.Services.AddScoped<IDailyContextService, DailyContextService>();
builder.Services.AddScoped<IWasteCategoryService, WasteCategoryService>();
builder.Services.AddScoped<IRawMaterialService, RawMaterialService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IProductionCaptureService, ProductionCaptureService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IExportService, ExportService>();

// ── Domain Events ─────────────────────────────────────────────
builder.Services.AddSingleton<DomainEventDispatcher>();
builder.Services.AddHostedService<DomainEventBackgroundWorker>();
builder.Services.AddScoped<IDomainEventHandler<SaleCompletedEvent>, SaleCompletedInventoryHandler>();

// ── Validation ─────────────────────────────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<ProductRequestValidator>();

// ── Controllers & Views ────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── FluentValidation auto-validation ───────────────────────────
builder.Services.AddFluentValidationAutoValidation();

// ── Health Checks ──────────────────────────────────────────────
builder.Services.AddHealthChecks();

var app = builder.Build();

// ── Middleware Pipeline ────────────────────────────────────────
app.UseSerilogRequestLogging();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ── Web frontend fallback: / → Dashboard ──────────────────────
app.MapControllerRoute(
    name: "web",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.MapHealthChecks("/health");

// ── Database setup (auto-create + seed) ────────────────────────
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PanaDbContext>();

    // EnsureCreated does nothing if DB already exists (created by PostgreSQL container).
    // Check if tables actually exist first.
    try
    {
        var _ = await db.Tenants.AnyAsync();
    }
    catch
    {
        // Tables don't exist — drop and recreate
        await db.Database.EnsureDeletedAsync();
    }

    await db.Database.EnsureCreatedAsync();

    var tenantExists = await db.Tenants.AnyAsync();
    if (!tenantExists)
    {
        var defaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        db.Tenants.Add(new Pana.Api.Domain.Common.Tenant(defaultTenantId, "Default Bakery", "default-bakery"));
        await db.SaveChangesAsync();
    }
}

app.Run();

