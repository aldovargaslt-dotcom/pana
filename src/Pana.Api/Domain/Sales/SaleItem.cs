namespace Pana.Api.Domain.Sales;

using Pana.Api.Domain.Common;

/// <summary>
/// A line item within a sale. Points to a product but stores the snapshot data.
/// </summary>
public class SaleItem : BaseEntity
{
    public Guid SaleId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal LineTotal => UnitPrice * Quantity;
    public bool IsVoided { get; private set; }

    private SaleItem() { } // EF Core

    public SaleItem(Guid saleId, Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        if (unitPrice < 0) throw new ArgumentException("Price cannot be negative.", nameof(unitPrice));

        SaleId = saleId;
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public void Void()
    {
        IsVoided = true;
        MarkUpdated();
    }
}
