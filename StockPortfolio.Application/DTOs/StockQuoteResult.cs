namespace StockPortfolio.Application.DTOs
{
    public class StockQuoteResult
    {
        public bool Success { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
