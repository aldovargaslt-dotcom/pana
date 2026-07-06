using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pana.Api.Application.Inventory;
using Pana.Api.Application.Sales;
using Pana.Api.Domain.Common;
using Pana.Api.Domain.Inventory;
using Pana.Api.Domain.Products;
using Pana.Api.Domain.Sales;
using Pana.Api.Infrastructure.Data;

namespace Pana.Tests.Integration;

/// <summary>
/// Integration tests for the sale → inventory deduction flow.
/// Uses EF Core InMemory database to test the full pipeline:
/// Create sale → Confirm → Deliver → Verify InventoryMovement created.
/// </summary>
public class SaleInventoryFlowTests : IDisposable
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private readonly PanaDbContext _db;
    private readonly FakeTenantContext _tenantContext;
    private readonly DomainEventDispatcher _eventDispatcher;
    private readonly SalesService _salesService;
    private readonly InventoryService _inventoryService;

    public SaleInventoryFlowTests()
    {
        var options = new DbContextOptionsBuilder<PanaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB per test
            .Options;

        _tenantContext = new FakeTenantContext { TenantId = TenantId };
        _db = new PanaDbContext(options, _tenantContext);
        _eventDispatcher = new DomainEventDispatcher();
        _salesService = new SalesService(_db, _tenantContext, _eventDispatcher);
        _inventoryService = new InventoryService(_db, _tenantContext);

        SeedProduct();
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    // ═══════════════════════════════════════════════════════════
    // Tests
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public async Task DeliverSale_CreatesInventoryDeduction()
    {
        // Arrange: create and confirm a sale
        var request = new CreateSaleRequest(
            Items: [new CreateSaleItemRequest(ProductId, UnitPrice: 10m, Quantity: 3)],
            Notes: null, CustomerName: null,
            CustomerPhone: null, ScheduledDate: null,
            DepositAmount: 0, PaymentMethod: null, InternalNotes: null);

        var sale = await _salesService.CreateAsync(request);
        await _salesService.ConfirmAsync(sale.Id);
        await _salesService.StartProductionAsync(sale.Id);
        await _salesService.MarkReadyAsync(sale.Id);

        // Act: deliver
        var delivered = await _salesService.DeliverAsync(sale.Id);

        // Assert: sale is delivered
        Assert.True(delivered);
        var fetched = await _salesService.GetByIdAsync(sale.Id);
        Assert.NotNull(fetched);
        Assert.Equal(Sale.Statuses.Delivered, fetched.Status);

        // Assert: inventory movement created (via domain event handler)
        // Note: domain events are async, so we call the handler directly for test determinism
        var handler = new SaleCompletedInventoryHandler(_inventoryService, new TestLogger<SaleCompletedInventoryHandler>());
        var saleEvent = new SaleCompletedEvent(
            sale.Id, TenantId,
            [new SaleItemSnapshot(ProductId, Quantity: 3)]);
        await handler.HandleAsync(saleEvent, CancellationToken.None);

        var movements = await _inventoryService.GetMovementsAsync(ProductId);
        Assert.NotEmpty(movements);
        var deduction = movements.FirstOrDefault(m => m.MovementType == InventoryMovement.Types.SaleDeduction);
        Assert.NotNull(deduction);
        Assert.Equal(-3m, deduction.Quantity);
        Assert.Equal(sale.Id, deduction.ReferenceSaleId);
    }

    [Fact]
    public async Task CancelledSale_DoesNotDeductInventory()
    {
        // Arrange
        var request = new CreateSaleRequest(
            Items: [new CreateSaleItemRequest(ProductId, UnitPrice: 10m, Quantity: 5)],
            Notes: null, CustomerName: null,
            CustomerPhone: null, ScheduledDate: null,
            DepositAmount: 0, PaymentMethod: null, InternalNotes: null);

        var sale = await _salesService.CreateAsync(request);
        await _salesService.ConfirmAsync(sale.Id);

        // Act: cancel instead of deliver
        var cancelled = await _salesService.CancelAsync(sale.Id);
        Assert.True(cancelled);

        // Assert: no SaleDeduction movement was created
        var movements = await _inventoryService.GetMovementsAsync(ProductId);
        Assert.DoesNotContain(movements, m => m.MovementType == InventoryMovement.Types.SaleDeduction);
    }

    [Fact]
    public async Task MultipleItems_EachDeductedCorrectly()
    {
        // Arrange: two products
        SeedProduct("Croissant", 15m);

        var request = new CreateSaleRequest(
            Items:
            [
                new CreateSaleItemRequest(ProductId, UnitPrice: 10m, Quantity: 2),
                new CreateSaleItemRequest(Product2Id, UnitPrice: 15m, Quantity: 4)
            ],
            Notes: null, CustomerName: null,
            CustomerPhone: null, ScheduledDate: null,
            DepositAmount: 0, PaymentMethod: null, InternalNotes: null);

        var sale = await _salesService.CreateAsync(request);
        await _salesService.ConfirmAsync(sale.Id);
        await _salesService.StartProductionAsync(sale.Id);
        await _salesService.MarkReadyAsync(sale.Id);
        await _salesService.DeliverAsync(sale.Id);

        // Act: dispatch the event
        var handler = new SaleCompletedInventoryHandler(_inventoryService, new TestLogger<SaleCompletedInventoryHandler>());
        var saleEvent = new SaleCompletedEvent(sale.Id, TenantId,
        [
            new SaleItemSnapshot(ProductId, Quantity: 2),
            new SaleItemSnapshot(Product2Id, Quantity: 4)
        ]);
        await handler.HandleAsync(saleEvent, CancellationToken.None);

        // Assert
        var movements1 = await _inventoryService.GetMovementsAsync(ProductId);
        var deduction1 = movements1.FirstOrDefault(m => m.MovementType == InventoryMovement.Types.SaleDeduction);
        Assert.NotNull(deduction1);
        Assert.Equal(-2m, deduction1.Quantity);

        var movements2 = await _inventoryService.GetMovementsAsync(Product2Id);
        var deduction2 = movements2.FirstOrDefault(m => m.MovementType == InventoryMovement.Types.SaleDeduction);
        Assert.NotNull(deduction2);
        Assert.Equal(-4m, deduction2.Quantity);
    }

    [Fact]
    public async Task DeliverTwice_ThrowsInvalidOperation()
    {
        var request = new CreateSaleRequest(
            Items: [new CreateSaleItemRequest(ProductId, UnitPrice: 10m, Quantity: 1)],
            Notes: null, CustomerName: null,
            CustomerPhone: null, ScheduledDate: null,
            DepositAmount: 0, PaymentMethod: null, InternalNotes: null);

        var sale = await _salesService.CreateAsync(request);
        await _salesService.ConfirmAsync(sale.Id);
        await _salesService.StartProductionAsync(sale.Id);
        await _salesService.MarkReadyAsync(sale.Id);
        await _salesService.DeliverAsync(sale.Id);

        // Second deliver should fail — already delivered
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _salesService.DeliverAsync(sale.Id));
    }

    [Fact]
    public async Task ConfirmWithoutDeliver_NoInventoryMovement()
    {
        var request = new CreateSaleRequest(
            Items: [new CreateSaleItemRequest(ProductId, UnitPrice: 10m, Quantity: 2)],
            Notes: null, CustomerName: null,
            CustomerPhone: null, ScheduledDate: null,
            DepositAmount: 0, PaymentMethod: null, InternalNotes: null);

        var sale = await _salesService.CreateAsync(request);
        await _salesService.ConfirmAsync(sale.Id);

        // Sale is Confirmed but not Delivered → no inventory deduction yet
        var movements = await _inventoryService.GetMovementsAsync(ProductId);
        Assert.DoesNotContain(movements, m => m.MovementType == InventoryMovement.Types.SaleDeduction);
    }

    [Fact]
    public async Task StockLevels_ReflectSaleDeduction()
    {
        // Stock in 10 units
        await _inventoryService.StockInAsync(
            new StockInRequest(ProductId, Quantity: 10, Reason: "Initial stock"));

        // Create and deliver a sale for 3 units
        var request = new CreateSaleRequest(
            Items: [new CreateSaleItemRequest(ProductId, UnitPrice: 10m, Quantity: 3)],
            Notes: null, CustomerName: null,
            CustomerPhone: null, ScheduledDate: null,
            DepositAmount: 0, PaymentMethod: null, InternalNotes: null);

        var sale = await _salesService.CreateAsync(request);
        await _salesService.ConfirmAsync(sale.Id);
        await _salesService.StartProductionAsync(sale.Id);
        await _salesService.MarkReadyAsync(sale.Id);
        await _salesService.DeliverAsync(sale.Id);

        var handler = new SaleCompletedInventoryHandler(_inventoryService, new TestLogger<SaleCompletedInventoryHandler>());
        await handler.HandleAsync(
            new SaleCompletedEvent(sale.Id, TenantId, [new SaleItemSnapshot(ProductId, Quantity: 3)]),
            CancellationToken.None);

        // Verify both movements exist and net to 7
        var movements = await _inventoryService.GetMovementsAsync(ProductId, limit: 100);
        var stockIn = movements.FirstOrDefault(m => m.MovementType == InventoryMovement.Types.StockIn);
        var deduction = movements.FirstOrDefault(m => m.MovementType == InventoryMovement.Types.SaleDeduction);

        Assert.NotNull(stockIn);
        Assert.Equal(10m, stockIn.Quantity);
        Assert.NotNull(deduction);
        Assert.Equal(-3m, deduction.Quantity);

        var netStock = movements.Sum(m => m.Quantity);
        Assert.Equal(7m, netStock);
    }

    // ═══════════════════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════════════════

    private static readonly Guid ProductId = Guid.Parse("a0000000-0000-0000-0000-000000000001");
    private static readonly Guid Product2Id = Guid.Parse("a0000000-0000-0000-0000-000000000002");

    private void SeedProduct(string name = "Concha de Vainilla", decimal price = 10m)
    {
        var productId = name == "Concha de Vainilla" ? ProductId : Product2Id;
        if (_db.Products.Any(p => p.Id == productId)) return;

        _db.Products.Add(new Product(
            TenantId,
            name,
            $"SKU-{name[..3].ToUpper()}",
            price,
            cost: price * 0.4m));

        _db.SaveChanges();
    }
}

/// <summary>
/// Fake tenant context for integration tests — returns a fixed TenantId.
/// </summary>
public class FakeTenantContext : ITenantContext
{
    public Guid TenantId { get; set; } = Guid.Parse("00000000-0000-0000-0000-000000000001");
}

/// <summary>
/// Simple null logger for integration tests — avoids NuGet dependency.
/// </summary>
public class TestLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}
