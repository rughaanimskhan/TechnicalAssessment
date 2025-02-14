namespace WebApplication1.IServices
{
    public interface ICurrencyProvider
    {
        Task<object> GetLatestRates(string baseCurrency);
        Task<object> ConvertCurrency(string from, string to, decimal amount);
        Task<object> GetHistoricalRates(string startDate, string endDate, string baseCurrency);
    }
}
