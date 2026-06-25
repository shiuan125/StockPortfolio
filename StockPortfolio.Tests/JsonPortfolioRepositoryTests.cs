using StockPortfolio.Domain;
using StockPortfolio.Infrastructure.Persistence;

namespace StockPortfolio.Tests;

public class JsonPortfolioRepositoryTests : IDisposable
{
    private readonly string _tempDir;
    private readonly JsonPortfolioRepository _repo;

    public JsonPortfolioRepositoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _repo = new JsonPortfolioRepository(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private static StockHolding NewHolding(string symbol = "2330") => new()
    {
        Id = Guid.NewGuid(),
        Symbol = symbol,
        Shares = 1000,
        PurchasePrice = 500m,
        PurchaseDate = new DateTime(2024, 1, 1)
    };

    // ── GetAll ────────────────────────────────────────────────────────────────

    [Fact]
    public void GetAll_ReturnsEmpty_WhenFileDoesNotExist()
    {
        var result = _repo.GetAll();
        Assert.Empty(result);
    }

    // ── Add ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Add_PersistsHolding_CanBeRetrievedWithGetAll()
    {
        var holding = NewHolding();
        _repo.Add(holding);

        var result = _repo.GetAll();
        Assert.Single(result);
        Assert.Equal(holding.Id, result[0].Id);
        Assert.Equal("2330", result[0].Symbol);
    }

    [Fact]
    public void Add_MultipleHoldings_AllPersisted()
    {
        _repo.Add(NewHolding("2330"));
        _repo.Add(NewHolding("0050"));
        _repo.Add(NewHolding("2317"));

        Assert.Equal(3, _repo.GetAll().Count);
    }

    [Fact]
    public void Add_PreservesAllFields()
    {
        var holding = new StockHolding
        {
            Id = Guid.NewGuid(),
            Symbol = "2330",
            Shares = 1234,
            PurchasePrice = 567.89m,
            PurchaseDate = new DateTime(2023, 6, 15)
        };
        _repo.Add(holding);

        var saved = _repo.GetAll()[0];
        Assert.Equal(holding.Id, saved.Id);
        Assert.Equal(holding.Symbol, saved.Symbol);
        Assert.Equal(holding.Shares, saved.Shares);
        Assert.Equal(holding.PurchasePrice, saved.PurchasePrice);
        Assert.Equal(holding.PurchaseDate, saved.PurchaseDate);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public void Delete_RemovesMatchingHolding()
    {
        var h1 = NewHolding("2330");
        var h2 = NewHolding("0050");
        _repo.Add(h1);
        _repo.Add(h2);

        _repo.Delete(h1.Id);

        var remaining = _repo.GetAll();
        Assert.Single(remaining);
        Assert.Equal(h2.Id, remaining[0].Id);
    }

    [Fact]
    public void Delete_DoesNothing_WhenIdNotFound()
    {
        _repo.Add(NewHolding());
        _repo.Delete(Guid.NewGuid());

        Assert.Single(_repo.GetAll());
    }

    [Fact]
    public void Delete_ResultsInEmpty_WhenLastHoldingRemoved()
    {
        var h = NewHolding();
        _repo.Add(h);
        _repo.Delete(h.Id);

        Assert.Empty(_repo.GetAll());
    }

    // ── Round-trip ────────────────────────────────────────────────────────────

    [Fact]
    public void AddAndDelete_RoundTrip_LeavesCorrectState()
    {
        var h1 = NewHolding("2330");
        var h2 = NewHolding("0050");
        var h3 = NewHolding("2317");

        _repo.Add(h1);
        _repo.Add(h2);
        _repo.Add(h3);
        _repo.Delete(h2.Id);

        var result = _repo.GetAll();
        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, r => r.Id == h2.Id);
    }
}
