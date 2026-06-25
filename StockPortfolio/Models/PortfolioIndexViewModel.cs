namespace StockPortfolio.Models
{
    public class PortfolioIndexViewModel
    {
        public List<StockHoldingViewModel> Holdings { get; set; } = new();
        public AddStockRequest NewHolding { get; set; } = new();

        public List<StockGroupViewModel> GroupedHoldings =>
            Holdings
                .GroupBy(h => h.Symbol)
                .Select(g => new StockGroupViewModel
                {
                    Symbol = g.Key,
                    Name = g.First().Name,
                    Holdings = g.OrderBy(h => h.PurchaseDate).ToList()
                })
                .OrderBy(g => g.Symbol)
                .ToList();

        public decimal TotalCost => Holdings.Sum(h => h.TotalCost);

        public decimal TotalCurrentValue =>
            Holdings.Where(h => h.CurrentValue.HasValue).Sum(h => h.CurrentValue!.Value);

        public decimal TotalPnL => TotalCurrentValue - TotalCost;

        public decimal TotalPnLPercent =>
            TotalCost > 0 ? TotalPnL / TotalCost * 100 : 0;

        public bool HasAnyPrice => Holdings.Any(h => h.CurrentPrice.HasValue);
    }
}
