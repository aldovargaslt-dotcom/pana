using Pana.Api.Domain.Sales;

namespace Pana.Tests.Domain;

public class SaleTests
{
    private static readonly Guid TenantId = Guid.NewGuid();

    [Fact]
    public void Create_ShouldHaveCompletedStatus()
    {
        var sale = new Sale(TenantId);

        Assert.Equal(Sale.Statuses.Completed, sale.Status);
        Assert.Equal(0m, sale.TotalAmount);
    }

    [Fact]
    public void AddItem_ShouldRecalculateTotal()
    {
        var sale = new Sale(TenantId);

        sale.AddItem(Guid.NewGuid(), "Croissant", 15.0m, 3);
        sale.AddItem(Guid.NewGuid(), "Café", 25.0m, 2);

        Assert.Equal(95.0m, sale.TotalAmount); // 15*3 + 25*2
    }

    [Fact]
    public void Void_CompletedSale_ShouldChangeStatus()
    {
        var sale = new Sale(TenantId);
        sale.AddItem(Guid.NewGuid(), "Test", 10.0m, 1);

        sale.Void(Guid.NewGuid());

        Assert.Equal(Sale.Statuses.Voided, sale.Status);
    }

    [Fact]
    public void AddItem_VoidedSale_ShouldThrow()
    {
        var sale = new Sale(TenantId);
        sale.Void(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() =>
            sale.AddItem(Guid.NewGuid(), "Test", 10.0m, 1));
    }

    [Fact]
    public void AddItem_ZeroQuantity_ShouldThrow()
    {
        var sale = new Sale(TenantId);

        Assert.Throws<ArgumentException>(() =>
            sale.AddItem(Guid.NewGuid(), "Test", 10.0m, 0));
    }

    [Fact]
    public void AddItem_NegativeQuantity_ShouldThrow()
    {
        var sale = new Sale(TenantId);

        Assert.Throws<ArgumentException>(() =>
            sale.AddItem(Guid.NewGuid(), "Test", 10.0m, -1));
    }
}
