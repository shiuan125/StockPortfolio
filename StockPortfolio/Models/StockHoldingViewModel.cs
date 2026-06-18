namespace StockPortfolio.Models
{
    public class StockHoldingViewModel
    {
        public Guid Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Shares { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal TotalCost => Shares * PurchasePrice;
        public decimal? CurrentPrice { get; set; }
        public decimal? CurrentValue => CurrentPrice.HasValue ? Shares * CurrentPrice.Value : null;
        public decimal? PnL => CurrentValue.HasValue ? CurrentValue.Value - TotalCost : null;
        public decimal? PnLPercent => TotalCost > 0 && PnL.HasValue ? PnL.Value / TotalCost * 100 : null;
        public string? ErrorMessage { get; set; }
    }
}
