namespace StockPortfolio.Application.DTOs
{
    public class StockCandlesResult
    {
        public bool Success { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public List<CandleData> Candles { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class CandleData
    {
        public string Date { get; set; } = string.Empty;
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
        public decimal? Change { get; set; }
    }
}
