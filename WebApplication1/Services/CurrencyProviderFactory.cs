using Microsoft.Extensions.Caching.Memory;
using WebApplication1.IServices;

namespace WebApplication1.Services
{
    public class CurrencyProviderFactory : ICurrencyProviderFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        public CurrencyProviderFactory(IMemoryCache cache, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
        }

        public ICurrencyProvider GetProvider(string providerName)
        {
            switch (providerName.ToLower())
            {
                case "frankfurterpayment":
                    return new FrankFurterProvider(_cache, _httpClientFactory);
                case "planetpayment":
                    return new PlanetPaymentProvider();
                case "adyen":
                    return new AdyenProvider();
                case "stripe":
                    return new StripeProvider();
                default:
                    throw new ArgumentException("Unsupported provider", nameof(providerName));
            }
        }
    }
}
