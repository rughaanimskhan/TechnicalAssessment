using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.IServices;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyProviderFactory _factory;

    public CurrencyController(ICurrencyProviderFactory factory) => _factory = factory;

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestRates(string providerName, [FromQuery] string baseCurrency = "EUR")
    {
        var provider = _factory.GetProvider(providerName);
        return Ok(await provider.GetLatestRates(baseCurrency));
    }

    [HttpGet("convert")]
    public async Task<IActionResult> ConvertCurrency(string providerName, [FromQuery] string from, [FromQuery] string to, [FromQuery] decimal amount)
    {
        if (from == "TRY" || from == "PLN" || from == "THB" || from == "MXN" ||
            to == "TRY" || to == "PLN" || to == "THB" || to == "MXN")
        {
            return BadRequest("Unsupported currency");
        }

        var provider = _factory.GetProvider(providerName);
        var result = await provider.ConvertCurrency(from, to, amount);
        return Ok(result);
    }

    [HttpGet("historical")]
    public async Task<IActionResult> GetHistoricalRates(string providerName, [FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] string baseCurrency = "EUR")
    {
        var provider = _factory.GetProvider(providerName);
        return Ok(await provider.GetHistoricalRates(startDate, endDate, baseCurrency));
    }
}