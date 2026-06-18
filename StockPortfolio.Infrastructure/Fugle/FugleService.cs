using System.Text.Json;
using Microsoft.Extensions.Configuration;
using StockPortfolio.Application.DTOs;
using StockPortfolio.Application.Interfaces;

namespace StockPortfolio.Infrastructure.Fugle
{
    public class FugleService : IFugleService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        private const string QuoteBaseUrl = "https://api.fugle.tw/marketdata/v1.0/stock/intraday/quote/";
        private const string CandlesBaseUrl = "https://api.fugle.tw/marketdata/v1.0/stock/historical/candles/";

        public FugleService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Fugle:ApiKey"] ?? string.Empty;
        }

        public async Task<StockQuoteResult> GetQuoteAsync(string symbol)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                return new StockQuoteResult { Symbol = symbol, ErrorMessage = "未設定 Fugle API Key" };

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{QuoteBaseUrl}{symbol}");
                request.Headers.Add("X-API-KEY", _apiKey);

                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    return new StockQuoteResult { Symbol = symbol, ErrorMessage = "已超過 API 請求限制" };

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    return new StockQuoteResult { Symbol = symbol, ErrorMessage = $"API 錯誤 ({(int)response.StatusCode}): {body}" };
                }

                var json = await response.Content.ReadAsStringAsync();
                var quote = JsonSerializer.Deserialize<FugleQuoteResponse>(json);

                if (quote == null)
                    return new StockQuoteResult { Symbol = symbol, ErrorMessage = "無法解析 API 回應" };

                var price = quote.LastPrice ?? quote.ClosePrice;

                return new StockQuoteResult
                {
                    Success = price.HasValue,
                    Symbol = symbol,
                    Name = quote.Name,
                    Price = price,
                    ErrorMessage = price.HasValue ? null : "目前無成交價格"
                };
            }
            catch (Exception ex)
            {
                return new StockQuoteResult { Symbol = symbol, ErrorMessage = $"連線錯誤: {ex.Message}" };
            }
        }

        public async Task<StockCandlesResult> GetCandlesAsync(string symbol, DateTime from, DateTime to)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                return new StockCandlesResult { Symbol = symbol, ErrorMessage = "未設定 Fugle API Key" };

            try
            {
                var url = $"{CandlesBaseUrl}{symbol}?timeframe=D&from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("X-API-KEY", _apiKey);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    return new StockCandlesResult { Symbol = symbol, ErrorMessage = $"API 錯誤 ({(int)response.StatusCode}): {body}" };
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<FugleCandlesResponse>(json);

                return new StockCandlesResult
                {
                    Success = true,
                    Symbol = symbol,
                    Candles = result?.Data.Select(d => new CandleData
                    {
                        Date = d.Date,
                        Open = d.Open,
                        High = d.High,
                        Low = d.Low,
                        Close = d.Close,
                        Volume = d.Volume,
                        Change = d.Change
                    }).ToList() ?? new()
                };
            }
            catch (Exception ex)
            {
                return new StockCandlesResult { Symbol = symbol, ErrorMessage = $"連線錯誤: {ex.Message}" };
            }
        }
    }
}
