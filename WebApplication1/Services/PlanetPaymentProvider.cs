using WebApplication1.IServices;

namespace WebApplication1.Services
{
    public class PlanetPaymentProvider : ICurrencyProvider
    {
        public Task<object> ConvertCurrency(string from, string to, decimal amount)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetHistoricalRates(string startDate, string endDate, string baseCurrency)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetLatestRates(string baseCurrency)
        {
            throw new NotImplementedException();
        }
    }
}
