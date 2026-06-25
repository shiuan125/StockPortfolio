using StockPortfolio.Models;

namespace StockPortfolio.Tests;

public class StockGroupViewModelTests
{
    private static StockHoldingViewModel Holding(
        decimal shares, decimal purchasePrice, decimal? currentPrice = null,
        string symbol = "2330", DateTime? date = null) => new()
    {
        Id = Guid.NewGuid(),
        Symbol = symbol,
        Name = "台積電",
        Shares = shares,
        PurchasePrice = purchasePrice,
        PurchaseDate = date ?? new DateTime(2024, 1, 1),
        CurrentPrice = currentPrice
    };

    private static StockGroupViewModel Group(params StockHoldingViewModel[] holdings) => new()
    {
        Symbol = "2330",
        Name = "台積電",
        Holdings = holdings.ToList()
    };

    // ── TotalShares ──────────────────────────────────────────────────────────

    [Fact]
    public void TotalShares_SumsAllHoldings()
    {
        var g = Group(Holding(1000, 500m), Holding(500, 480m));
        Assert.Equal(1500m, g.TotalShares);
    }

    // ── TotalCost ────────────────────────────────────────────────────────────

    [Fact]
    public void TotalCost_SumsSharesTimesPurchasePrice()
    {
        var g = Group(Holding(1000, 500m), Holding(500, 480m));
        Assert.Equal(740_000m, g.TotalCost);
    }

    // ── AvgPurchasePrice ─────────────────────────────────────────────────────

    [Fact]
    public void AvgPurchasePrice_IsWeightedAverage()
    {
        // 1000*500 + 500*480 = 740000 / 1500 ≈ 493.33
        var g = Group(Holding(1000, 500m), Holding(500, 480m));
        Assert.Equal(740_000m / 1500m, g.AvgPurchasePrice);
    }

    [Fact]
    public void AvgPurchasePrice_IsZero_WhenTotalSharesAreZero()
    {
        var g = Group(Holding(0, 500m));
        Assert.Equal(0m, g.AvgPurchasePrice);
    }

    // ── CurrentPrice ─────────────────────────────────────────────────────────

    [Fact]
    public void CurrentPrice_IsNull_WhenNoHoldingHasPrice()
    {
        var g = Group(Holding(1000, 500m, null));
        Assert.Null(g.CurrentPrice);
    }

    [Fact]
    public void CurrentPrice_ReturnFirstAvailablePrice()
    {
        var g = Group(Holding(1000, 500m, 600m));
        Assert.Equal(600m, g.CurrentPrice);
    }

    // ── CurrentValue / PnL / PnLPercent ──────────────────────────────────────

    [Fact]
    public void CurrentValue_IsNull_WhenNoPriceAvailable()
    {
        var g = Group(Holding(1000, 500m, null));
        Assert.Null(g.CurrentValue);
    }

    [Fact]
    public void CurrentValue_ReturnsTotalSharesTimesCurrentPrice()
    {
        var g = Group(Holding(1000, 500m, 600m), Holding(500, 480m, 600m));
        Assert.Equal(1500m * 600m, g.CurrentValue);
    }

    [Fact]
    public void PnL_ReturnsCurrentValueMinusTotalCost()
    {
        var g = Group(Holding(1000, 500m, 600m));
        // CurrentValue = 600000, TotalCost = 500000
        Assert.Equal(100_000m, g.PnL);
    }

    [Fact]
    public void PnLPercent_Returns20_WhenPriceRises20Percent_SingleHolding()
    {
        var g = Group(Holding(1000, 500m, 600m));
        Assert.Equal(20m, g.PnLPercent);
    }

    [Fact]
    public void PnLPercent_IsNull_WhenNoPriceAvailable()
    {
        var g = Group(Holding(1000, 500m, null));
        Assert.Null(g.PnLPercent);
    }

    [Fact]
    public void ErrorMessage_ReturnsFirstNonNullErrorFromHoldings()
    {
        var h1 = Holding(1000, 500m);
        var h2 = Holding(500, 480m);
        h2.ErrorMessage = "API 錯誤";
        var g = Group(h1, h2);
        Assert.Equal("API 錯誤", g.ErrorMessage);
    }
}
