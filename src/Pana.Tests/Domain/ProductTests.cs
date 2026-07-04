using Pana.Api.Domain.Products;

namespace Pana.Tests.Domain;

public class ProductTests
{
    private static readonly Guid TenantId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var product = new Product(TenantId, "Pan de muerto", "PDM-001", 25.0m, 10.0m);

        Assert.Equal("Pan de muerto", product.Name);
        Assert.Equal("PDM-001", product.Sku);
        Assert.Equal(25.0m, product.Price);
        Assert.Equal(10.0m, product.Cost);
        Assert.Equal(15.0m, product.Margin);
        Assert.Equal(60.0m, product.MarginPercentage);
        Assert.True(product.IsActive);
    }

    [Fact]
    public void SetPricing_NegativePrice_ShouldThrow()
    {
        var product = new Product(TenantId, "Test", "TST-001", 10.0m, 5.0m);

        Assert.Throws<ArgumentException>(() => product.SetPricing(-5.0m, 5.0m));
    }

    [Fact]
    public void SetPricing_NegativeCost_ShouldThrow()
    {
        var product = new Product(TenantId, "Test", "TST-001", 10.0m, 5.0m);

        Assert.Throws<ArgumentException>(() => product.SetPricing(10.0m, -5.0m));
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var product = new Product(TenantId, "Test", "TST-001", 10.0m, 5.0m);

        product.Deactivate();

        Assert.False(product.IsActive);
    }

    [Fact]
    public void Deactivate_ThenActivate_ShouldSetIsActiveTrue()
    {
        var product = new Product(TenantId, "Test", "TST-001", 10.0m, 5.0m);

        product.Deactivate();
        product.Activate();

        Assert.True(product.IsActive);
    }

    [Fact]
    public void Margin_WithZeroPrice_ShouldBeNegativeCost()
    {
        var product = new Product(TenantId, "Freebie", "FR-001", 0m, 5.0m);

        Assert.Equal(-5.0m, product.Margin);
    }
}
