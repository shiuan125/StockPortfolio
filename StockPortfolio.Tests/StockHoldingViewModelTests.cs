using StockPortfolio.Models;

namespace StockPortfolio.Tests;

public class StockHoldingViewModelTests
{
    private static StockHoldingViewModel Make(
        decimal shares = 1000,
        decimal purchasePrice = 100m,
        decimal? currentPrice = null) => new()
    {
        Id = Guid.NewGuid(),
        Symbol = "2330",
        Name = "台積電",
        Shares = shares,
        PurchasePrice = purchasePrice,
        PurchaseDate = new DateTime(2024, 1, 1),
        CurrentPrice = currentPrice
    };

    [Fact]
    public void TotalCost_ReturnsSharesTimePurchasePrice()
    {
        var vm = Make(shares: 1000, purchasePrice: 500m);
        Assert.Equal(500_000m, vm.TotalCost);
    }

    [Fact]
    public void CurrentValue_IsNull_WhenCurrentPriceNotSet()
    {
        var vm = Make(currentPrice: null);
        Assert.Null(vm.CurrentValue);
    }

    [Fact]
    public void CurrentValue_ReturnsSharesTimesCurrentPrice()
    {
        var vm = Make(shares: 1000, currentPrice: 600m);
        Assert.Equal(600_000m, vm.CurrentValue);
    }

    [Fact]
    public void PnL_IsNull_WhenCurrentPriceNotSet()
    {
        var vm = Make(currentPrice: null);
        Assert.Null(vm.PnL);
    }

    [Fact]
    public void PnL_ReturnsPositiveValue_WhenGain()
    {
        var vm = Make(shares: 1000, purchasePrice: 500m, currentPrice: 600m);
        Assert.Equal(100_000m, vm.PnL);
    }

    [Fact]
    public void PnL_ReturnsNegativeValue_WhenLoss()
    {
        var vm = Make(shares: 1000, purchasePrice: 500m, currentPrice: 400m);
        Assert.Equal(-100_000m, vm.PnL);
    }

    [Fact]
    public void PnLPercent_IsNull_WhenCurrentPriceNotSet()
    {
        var vm = Make(currentPrice: null);
        Assert.Null(vm.PnLPercent);
    }

    [Fact]
    public void PnLPercent_Returns20_WhenPriceRises20Percent()
    {
        var vm = Make(shares: 1000, purchasePrice: 500m, currentPrice: 600m);
        Assert.Equal(20m, vm.PnLPercent);
    }

    [Fact]
    public void PnLPercent_ReturnsNegative_WhenLoss()
    {
        var vm = Make(shares: 1000, purchasePrice: 500m, currentPrice: 400m);
        Assert.Equal(-20m, vm.PnLPercent);
    }

    [Fact]
    public void TotalCost_IsZero_WhenSharesAreZero()
    {
        var vm = Make(shares: 0, purchasePrice: 500m);
        Assert.Equal(0m, vm.TotalCost);
    }
}
