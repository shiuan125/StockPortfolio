using StockPortfolio.Domain;

namespace StockPortfolio.Application.Interfaces
{
    public interface IPortfolioRepository
    {
        List<StockHolding> GetAll();
        void Add(StockHolding holding);
        void Delete(Guid id);
    }
}
