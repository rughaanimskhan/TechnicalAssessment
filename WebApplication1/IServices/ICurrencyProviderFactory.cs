namespace WebApplication1.IServices
{
    public interface ICurrencyProviderFactory
    {
        ICurrencyProvider GetProvider(string providerName);
    }
}
