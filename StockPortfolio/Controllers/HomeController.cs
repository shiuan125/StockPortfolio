using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StockPortfolio.Application.Interfaces;
using StockPortfolio.Domain;
using StockPortfolio.Models;

namespace StockPortfolio.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPortfolioRepository _repository;
        private readonly IFugleService _fugleService;

        public HomeController(IPortfolioRepository repository, IFugleService fugleService)
        {
            _repository = repository;
            _fugleService = fugleService;
        }

        public async Task<IActionResult> Index()
        {
            var holdings = _repository.GetAll();

            var symbols = holdings.Select(h => h.Symbol).Distinct().ToList();
            var quoteTasks = symbols.ToDictionary(s => s, s => _fugleService.GetQuoteAsync(s));
            await Task.WhenAll(quoteTasks.Values);
            var quotes = quoteTasks.ToDictionary(kv => kv.Key, kv => kv.Value.Result);

            var viewModel = new PortfolioIndexViewModel
            {
                Holdings = holdings.Select(h =>
                {
                    var quote = quotes.TryGetValue(h.Symbol, out var q) ? q : null;
                    return new StockHoldingViewModel
                    {
                        Id = h.Id,
                        Symbol = h.Symbol,
                        Name = !string.IsNullOrEmpty(quote?.Name) ? quote.Name : h.Symbol,
                        Shares = h.Shares,
                        PurchasePrice = h.PurchasePrice,
                        PurchaseDate = h.PurchaseDate,
                        CurrentPrice = quote?.Price,
                        ErrorMessage = quote?.ErrorMessage
                    };
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add([Bind(Prefix = "NewHolding")] AddStockRequest request)
        {
            if (!ModelState.IsValid)
            {
                TempData["AddError"] = "請確認輸入資料正確（股票代號、股數、購入成本、購入日期均為必填）";
                return RedirectToAction(nameof(Index));
            }

            _repository.Add(new StockHolding
            {
                Symbol = request.Symbol.Trim().ToUpper(),
                Shares = request.Shares,
                PurchasePrice = request.PurchasePrice,
                PurchaseDate = request.PurchaseDate
            });

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Guid id)
        {
            _repository.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Chart(string symbol, decimal? purchasePrice)
        {
            ViewBag.Symbol = symbol?.ToUpper() ?? string.Empty;
            ViewBag.PurchasePrice = purchasePrice;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> History(string symbol)
        {
            var to = DateTime.Today;
            var from = to.AddDays(-364);

            var candlesTask = _fugleService.GetCandlesAsync(symbol, from, to);
            var quoteTask = _fugleService.GetQuoteAsync(symbol);
            await Task.WhenAll(candlesTask, quoteTask);
            var candlesResult = candlesTask.Result;
            var quoteResult = quoteTask.Result;

            if (!candlesResult.Success)
                return Json(new { error = candlesResult.ErrorMessage });

            return Json(new
            {
                symbol = symbol.ToUpper(),
                name = quoteResult.Name ?? symbol,
                dates = candlesResult.Candles.Select(c => c.Date).ToArray(),
                prices = candlesResult.Candles.Select(c => c.Close).ToArray()
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
