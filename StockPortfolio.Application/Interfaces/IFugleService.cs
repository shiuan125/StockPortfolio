using StockPortfolio.Application.DTOs;

namespace StockPortfolio.Application.Interfaces
{
    public interface IFugleService
    {
        Task<StockQuoteResult> GetQuoteAsync(string symbol);
        Task<StockCandlesResult> GetCandlesAsync(string symbol, DateTime from, DateTime to);
    }
}
