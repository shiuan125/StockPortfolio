using System.Text.Json;
using StockPortfolio.Application.Interfaces;
using StockPortfolio.Domain;

namespace StockPortfolio.Infrastructure.Persistence
{
    public class JsonPortfolioRepository : IPortfolioRepository
    {
        private readonly string _filePath;
        private readonly object _lock = new();

        public JsonPortfolioRepository(string dataDirectory)
        {
            Directory.CreateDirectory(dataDirectory);
            _filePath = Path.Combine(dataDirectory, "portfolio.json");
        }

        private List<StockHolding> ReadFromFile()
        {
            if (!File.Exists(_filePath)) return new();
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<StockHolding>>(json) ?? new();
        }

        private void WriteToFile(List<StockHolding> holdings)
        {
            File.WriteAllText(_filePath, JsonSerializer.Serialize(holdings));
        }

        public List<StockHolding> GetAll()
        {
            lock (_lock) return ReadFromFile();
        }

        public void Add(StockHolding holding)
        {
            lock (_lock)
            {
                var holdings = ReadFromFile();
                holdings.Add(holding);
                WriteToFile(holdings);
            }
        }

        public void Delete(Guid id)
        {
            lock (_lock)
            {
                var holdings = ReadFromFile();
                holdings.RemoveAll(h => h.Id == id);
                WriteToFile(holdings);
            }
        }
    }
}
