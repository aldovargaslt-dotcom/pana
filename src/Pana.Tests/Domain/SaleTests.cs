using Pana.Api.Domain.Sales;

namespace Pana.Tests.Domain;

public class SaleTests
{
    private static readonly Guid TenantId = Guid.NewGuid();

    [Fact]
    public void Create_ShouldStartAsDraft()
    {
        var sale = new Sale(TenantId);

        Assert.Equal(Sale.Statuses.Draft, sale.Status);
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
    public void Cancel_DraftSale_ShouldChangeStatus()
    {
        var sale = new Sale(TenantId);
        sale.AddItem(Guid.NewGuid(), "Test", 10.0m, 1);

        sale.Cancel();

        Assert.Equal(Sale.Statuses.Cancelled, sale.Status);
    }

    [Fact]
    public void AddItem_CancelledSale_ShouldThrow()
    {
        var sale = new Sale(TenantId);
        sale.Cancel();

        Assert.Throws<InvalidOperationException>(() =>
            sale.AddItem(Guid.NewGuid(), "Test", 10.0m, 1));
    }

    [Fact]
    public void FullStateMachine_ShouldTransitionCorrectly()
    {
        var sale = new Sale(TenantId);
        sale.AddItem(Guid.NewGuid(), "Bread", 5.0m, 2);

        Assert.Equal(Sale.Statuses.Draft, sale.Status);

        sale.Confirm();
        Assert.Equal(Sale.Statuses.Confirmed, sale.Status);

        sale.StartProduction();
        Assert.Equal(Sale.Statuses.InProduction, sale.Status);

        sale.MarkReady();
        Assert.Equal(Sale.Statuses.Ready, sale.Status);

        sale.Deliver();
        Assert.Equal(Sale.Statuses.Delivered, sale.Status);
    }

    [Fact]
    public void InvalidTransition_ShouldThrow()
    {
        var sale = new Sale(TenantId);
        // Draft → InProduction is not allowed (must go through Confirmed)
        Assert.Throws<InvalidOperationException>(() => sale.StartProduction());
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
