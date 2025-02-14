using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using WebApplication1.IServices;

namespace WebApplication1.Services
{
    public class FrankFurterProvider : ICurrencyProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache; 

        public FrankFurterProvider(IMemoryCache cache, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache; 
        }

        public async Task<object> GetLatestRates(string baseCurrency)
        {
            var _httpClient = _httpClientFactory.CreateClient("MyHttpClient");

            var response = await _httpClient.GetAsync($"latest?from={baseCurrency}");
            response.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<object>(await response.Content.ReadAsStringAsync());
        }

        private async Task<object> GetExchangeRateAsync(string from, string to, decimal amount)
        {
            var _httpClient = _httpClientFactory.CreateClient("MyHttpClient");

            var response = await _httpClient.GetStringAsync($"latest?from={from}&to={to}");
            var rates = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

            if (rates.TryGetValue("rates", out var ratesData))
            {
                var conversionRate = JsonSerializer.Deserialize<Dictionary<string, decimal>>(ratesData.ToString());
                decimal convertedAmount = amount * conversionRate[to];
                return new { Amount = amount, ConvertedAmount = convertedAmount, Currency = to };
            }

            return null;
        }

        public async Task<object> ConvertCurrency(string from, string to, decimal amount)
        {
            var cacheKey = $"{from}_{to}";
            if (!_cache.TryGetValue(cacheKey, out object rate))
            {
                rate = await GetExchangeRateAsync(from, to, amount);
                _cache.Set(cacheKey, rate, TimeSpan.FromHours(1));
            }
            return rate;
        }

        public async Task<object> GetHistoricalRates(string startDate, string endDate, string baseCurrency)
        {
            var _httpClient = _httpClientFactory.CreateClient("MyHttpClient");
            var response = await _httpClient.GetAsync($"{startDate}..{endDate}?from={baseCurrency}");
            response.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<object>(await response.Content.ReadAsStringAsync());
        }
    }
}
