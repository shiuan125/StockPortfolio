using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using StockPortfolio.Application.DTOs;
using StockPortfolio.Application.Interfaces;
using StockPortfolio.Controllers;
using StockPortfolio.Domain;
using StockPortfolio.Models;

namespace StockPortfolio.Tests;

public class HomeControllerTests
{
    private readonly Mock<IPortfolioRepository> _repoMock = new();
    private readonly Mock<IFugleService> _fugleMock = new();

    private HomeController CreateController()
    {
        var controller = new HomeController(_repoMock.Object, _fugleMock.Object);

        // TempData 需要 HttpContext
        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        return controller;
    }

    private static StockHolding Holding(string symbol = "2330") => new()
    {
        Id = Guid.NewGuid(),
        Symbol = symbol,
        Shares = 1000,
        PurchasePrice = 500m,
        PurchaseDate = new DateTime(2024, 1, 1)
    };

    // ── Index ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Index_EmptyPortfolio_ReturnsViewWithEmptyHoldings()
    {
        _repoMock.Setup(r => r.GetAll()).Returns([]);

        var controller = CreateController();
        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<PortfolioIndexViewModel>(view.Model);
        Assert.Empty(vm.Holdings);
    }

    [Fact]
    public async Task Index_WithHoldings_CallsGetQuoteForEachDistinctSymbol()
    {
        _repoMock.Setup(r => r.GetAll()).Returns([Holding("2330"), Holding("2330"), Holding("0050")]);
        _fugleMock.Setup(f => f.GetQuoteAsync(It.IsAny<string>()))
                  .ReturnsAsync(new StockQuoteResult { Success = true, Price = 600m, Name = "Test" });

        await CreateController().Index();

        // 2330 和 0050 各呼叫一次（distinct）
        _fugleMock.Verify(f => f.GetQuoteAsync("2330"), Times.Once);
        _fugleMock.Verify(f => f.GetQuoteAsync("0050"), Times.Once);
    }

    [Fact]
    public async Task Index_QuoteSuccess_SetsCurrentPriceAndName()
    {
        var holding = Holding("2330");
        _repoMock.Setup(r => r.GetAll()).Returns([holding]);
        _fugleMock.Setup(f => f.GetQuoteAsync("2330"))
                  .ReturnsAsync(new StockQuoteResult { Success = true, Price = 630m, Name = "台積電" });

        var result = await CreateController().Index();

        var vm = (PortfolioIndexViewModel)((ViewResult)result).Model!;
        Assert.Equal(630m, vm.Holdings[0].CurrentPrice);
        Assert.Equal("台積電", vm.Holdings[0].Name);
    }

    [Fact]
    public async Task Index_QuoteFailure_CurrentPriceIsNull()
    {
        _repoMock.Setup(r => r.GetAll()).Returns([Holding("2330")]);
        _fugleMock.Setup(f => f.GetQuoteAsync("2330"))
                  .ReturnsAsync(new StockQuoteResult { Success = false, ErrorMessage = "API 錯誤" });

        var result = await CreateController().Index();

        var vm = (PortfolioIndexViewModel)((ViewResult)result).Model!;
        Assert.Null(vm.Holdings[0].CurrentPrice);
        Assert.Equal("API 錯誤", vm.Holdings[0].ErrorMessage);
    }

    [Fact]
    public async Task Index_QuoteReturnsNullName_FallsBackToSymbol()
    {
        var holding = Holding("2330");
        _repoMock.Setup(r => r.GetAll()).Returns([holding]);
        _fugleMock.Setup(f => f.GetQuoteAsync("2330"))
                  .ReturnsAsync(new StockQuoteResult { Success = true, Price = 600m, Name = null });

        var result = await CreateController().Index();

        var vm = (PortfolioIndexViewModel)((ViewResult)result).Model!;
        Assert.Equal("2330", vm.Holdings[0].Name);
    }

    // ── Add ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Add_ValidRequest_AddsHoldingAndRedirects()
    {
        var request = new AddStockRequest
        {
            Symbol = " 2330 ",
            Shares = 1000,
            PurchasePrice = 500m,
            PurchaseDate = new DateTime(2024, 1, 1)
        };

        var controller = CreateController();
        var result = controller.Add(request);

        _repoMock.Verify(r => r.Add(It.Is<StockHolding>(h =>
            h.Symbol == "2330" &&
            h.Shares == 1000 &&
            h.PurchasePrice == 500m)), Times.Once);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public void Add_ValidRequest_SymbolIsUppercasedAndTrimmed()
    {
        var request = new AddStockRequest
        {
            Symbol = " tsm ",
            Shares = 100,
            PurchasePrice = 100m,
            PurchaseDate = DateTime.Today
        };

        CreateController().Add(request);

        _repoMock.Verify(r => r.Add(It.Is<StockHolding>(h => h.Symbol == "TSM")), Times.Once);
    }

    [Fact]
    public void Add_InvalidModelState_SetsTempDataAndRedirects()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("Symbol", "必填");

        var result = controller.Add(new AddStockRequest());

        _repoMock.Verify(r => r.Add(It.IsAny<StockHolding>()), Times.Never);
        Assert.NotNull(controller.TempData["AddError"]);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public void Delete_CallsRepositoryDeleteAndRedirects()
    {
        var id = Guid.NewGuid();
        var controller = CreateController();

        var result = controller.Delete(id);

        _repoMock.Verify(r => r.Delete(id), Times.Once);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    // ── Chart ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Chart_SetsViewBagSymbolUppercase()
    {
        var result = CreateController().Chart("tsm", 150m);

        Assert.IsType<ViewResult>(result);
        // Chart() sets ViewBag on the controller instance, check via controller
        var controller = CreateController();
        controller.Chart("tsm", 150m);
        Assert.Equal("TSM", controller.ViewBag.Symbol);
    }

    [Fact]
    public void Chart_SetsViewBagPurchasePrice()
    {
        var controller = CreateController();
        controller.Chart("2330", 500m);

        Assert.Equal(500m, controller.ViewBag.PurchasePrice);
    }

    [Fact]
    public void Chart_NullSymbol_SetsEmptyString()
    {
        var controller = CreateController();
        controller.Chart(null!, null);

        Assert.Equal(string.Empty, controller.ViewBag.Symbol);
    }

    // ── History ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task History_ReturnsJsonWithDatesAndPrices_WhenSuccess()
    {
        _fugleMock.Setup(f => f.GetCandlesAsync("2330", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                  .ReturnsAsync(new StockCandlesResult
                  {
                      Success = true,
                      Symbol = "2330",
                      Candles = [new CandleData { Date = "2024-01-02", Close = 600m }]
                  });
        _fugleMock.Setup(f => f.GetQuoteAsync("2330"))
                  .ReturnsAsync(new StockQuoteResult { Success = true, Name = "台積電" });

        var result = await CreateController().History("2330");

        var json = Assert.IsType<JsonResult>(result);
        var data = json.Value!;
        var type = data.GetType();
        Assert.Equal("2330", type.GetProperty("symbol")?.GetValue(data)?.ToString());
        Assert.Equal("台積電", type.GetProperty("name")?.GetValue(data)?.ToString());
    }

    [Fact]
    public async Task History_ReturnsErrorJson_WhenCandlesFail()
    {
        _fugleMock.Setup(f => f.GetCandlesAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                  .ReturnsAsync(new StockCandlesResult { Success = false, ErrorMessage = "無資料" });
        _fugleMock.Setup(f => f.GetQuoteAsync(It.IsAny<string>()))
                  .ReturnsAsync(new StockQuoteResult());

        var result = await CreateController().History("9999");

        var json = Assert.IsType<JsonResult>(result);
        var error = json.Value!.GetType().GetProperty("error")?.GetValue(json.Value)?.ToString();
        Assert.Equal("無資料", error);
    }
}
