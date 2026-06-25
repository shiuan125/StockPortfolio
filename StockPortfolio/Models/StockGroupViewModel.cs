namespace StockPortfolio.Models
{
    public class StockGroupViewModel
    {
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<StockHoldingViewModel> Holdings { get; set; } = new();

        public decimal TotalShares => Holdings.Sum(h => h.Shares);
        public decimal TotalCost => Holdings.Sum(h => h.TotalCost);
        public decimal AvgPurchasePrice => TotalShares > 0 ? TotalCost / TotalShares : 0;
        public decimal? CurrentPrice => Holdings.FirstOrDefault(h => h.CurrentPrice.HasValue)?.CurrentPrice;
        public decimal? CurrentValue => CurrentPrice.HasValue ? TotalShares * CurrentPrice.Value : null;
        public decimal? PnL => CurrentValue.HasValue ? CurrentValue.Value - TotalCost : null;
        public decimal? PnLPercent => TotalCost > 0 && PnL.HasValue ? PnL.Value / TotalCost * 100 : null;
        public string? ErrorMessage => Holdings.FirstOrDefault(h => h.ErrorMessage != null)?.ErrorMessage;
    }
}
