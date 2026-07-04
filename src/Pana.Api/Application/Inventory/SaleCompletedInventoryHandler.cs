namespace Pana.Api.Application.Inventory;

using Pana.Api.Domain.Common;
using Pana.Api.Domain.Sales;

/// <summary>
/// Listens for sale completion events and deducts inventory.
/// </summary>
public class SaleCompletedInventoryHandler : IDomainEventHandler<SaleCompletedEvent>
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<SaleCompletedInventoryHandler> _logger;

    public SaleCompletedInventoryHandler(IInventoryService inventoryService, ILogger<SaleCompletedInventoryHandler> logger)
    {
        _inventoryService = inventoryService;
        _logger = logger;
    }

    public async Task HandleAsync(SaleCompletedEvent domainEvent, CancellationToken ct)
    {
        _logger.LogInformation("Deducting inventory for sale {SaleId} with {ItemCount} items",
            domainEvent.SaleId, domainEvent.Items.Count);

        foreach (var item in domainEvent.Items)
        {
            await _inventoryService.DeductForSaleAsync(item.ProductId, item.Quantity, domainEvent.SaleId, ct);
        }

        _logger.LogInformation("Inventory deducted successfully for sale {SaleId}", domainEvent.SaleId);
    }
}
