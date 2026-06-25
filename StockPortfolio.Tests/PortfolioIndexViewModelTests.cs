using StockPortfolio.Models;

namespace StockPortfolio.Tests;

public class PortfolioIndexViewModelTests
{
    private static StockHoldingViewModel H(
        string symbol, decimal shares, decimal purchasePrice,
        decimal? currentPrice = null, DateTime? date = null) => new()
    {
        Id = Guid.NewGuid(),
        Symbol = symbol,
        Name = symbol,
        Shares = shares,
        PurchasePrice = purchasePrice,
        PurchaseDate = date ?? new DateTime(2024, 1, 1),
        CurrentPrice = currentPrice
    };

    // ── GroupedHoldings ───────────────────────────────────────────────────────

    [Fact]
    public void GroupedHoldings_EmptyWhenNoHoldings()
    {
        var vm = new PortfolioIndexViewModel();
        Assert.Empty(vm.GroupedHoldings);
    }

    [Fact]
    public void GroupedHoldings_SingleSymbolProducesOneGroup()
    {
        var vm = new PortfolioIndexViewModel
        {
            Holdings = [H("2330", 1000, 500m), H("2330", 500, 480m)]
        };
        Assert.Single(vm.GroupedHoldings);
        Assert.Equal(2, vm.GroupedHoldings[0].Holdings.Count);
    }

    [Fact]
    public void GroupedHoldings_MultipleSymbolsGroupedCorrectly()
    {
        var vm = new PortfolioIndexViewModel
        {
            Holdings = [H("2330", 1000, 500m), H("0050", 200, 150m), H("2330", 500, 480m)]
        };
        Assert.Equal(2, vm.GroupedHoldings.Count);
    }

    [Fact]
    public void GroupedHoldings_OrderedBySymbol()
    {
        var vm = new PortfolioIndexViewModel
        {
            Holdings = [H("2330", 1000, 500m), H("0050", 200, 150m)]
        };
        Assert.Equal("0050", vm.GroupedHoldings[0].Symbol);
        Assert.Equal("2330", vm.GroupedHoldings[1].Symbol);
    }

    [Fact]
    public void GroupedHoldings_HoldingsOrderedByPurchaseDate()
    {
        var older = H("2330", 500, 480m, date: new DateTime(2023, 6, 1));
        var newer = H("2330", 1000, 500m, date: new DateTime(2024, 1, 1));
        var vm = new PortfolioIndexViewModel { Holdings = [newer, older] };

        var group = vm.GroupedHoldings[0];
        Assert.Equal(older.PurchaseDate, group.Holdings[0].PurchaseDate);
    }

    // ── TotalCost ────────────────────────────────────────────────────────────

    [Fact]
    public void TotalCost_IsZeroWhenEmpty()
    {
        Assert.Equal(0m, new PortfolioIndexViewModel().TotalCost);
    }

    [Fact]
    public void TotalCost_SumsAllHoldings()
    {
        var vm = new PortfolioIndexViewModel
        {
            Holdings = [H("2330", 1000, 500m), H("0050", 200, 150m)]
        };
        Assert.Equal(530_000m, vm.TotalCost);
    }

    // ── HasAnyPrice ──────────────────────────────────────────────────────────

    [Fact]
    public void HasAnyPrice_FalseWhenNoPrices()
    {
        var vm = new PortfolioIndexViewModel
        {
            Holdings = [H("2330", 1000, 500m, null)]
        };
        Assert.False(vm.HasAnyPrice);
    }

    [Fact]
    public void HasAnyPrice_TrueWhenAtLeastOnePriceSet()
    {
        var vm = new PortfolioIndexViewModel
        {
            Holdings = [H("2330", 1000, 500m, 600m), H("0050", 200, 150m, null)]
        };
        Assert.True(vm.HasAnyPrice);
    }

    // ── TotalCurrentValue / TotalPnL / TotalPnLPercent ───────────────────────

    [Fact]
    public void TotalCurrentValue_SkipsHoldingsWithoutPrice()
    {
        var vm = new PortfolioIndexViewModel
        {
            Holdings = [H("2330", 1000, 500m, 600m), H("0050", 200, 150m, null)]
        };
        Assert.Equal(600_000m, vm.TotalCurrentValue);
    }

    [Fact]
    public void TotalPnL_ReturnsCurrentValueMinusTotalCost()
    {
        var vm = new PortfolioIndexViewModel
        {
            Holdings = [H("2330", 1000, 500m, 600m)]
        };
        Assert.Equal(100_000m, vm.TotalPnL);
    }

    [Fact]
    public void TotalPnLPercent_IsZeroWhenNoCost()
    {
        Assert.Equal(0m, new PortfolioIndexViewModel().TotalPnLPercent);
    }

    [Fact]
    public void TotalPnLPercent_Returns20_WhenPriceRise20Percent()
    {
        var vm = new PortfolioIndexViewModel
        {
            Holdings = [H("2330", 1000, 500m, 600m)]
        };
        Assert.Equal(20m, vm.TotalPnLPercent);
    }
}
