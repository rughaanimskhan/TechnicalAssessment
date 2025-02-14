using Moq;
using WebApplication1.IServices;
using Xunit;

namespace WebApplication1.UnitTest
{
    public class CurrencyServiceTests
    {
        [Fact]
        public async Task GetLatestRates_ShouldReturnData()
        {
            var mockService = new Mock<ICurrencyProvider>();
            mockService.Setup(s => s.GetLatestRates("EUR")).ReturnsAsync(new { Rate = 1.1 });

            var result = await mockService.Object.GetLatestRates("EUR");

            Assert.NotNull(result);
        }
    }
}
