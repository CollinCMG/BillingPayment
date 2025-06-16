using BillingPayment.Enums;
using BillingPayment.Interfaces;
using BillingPayment.Models;
using BillingPayment.Providers;
using BillingPayment.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

public class InvoiceServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactory = new();
    private readonly Mock<IConfiguration> _configuration = new();
    private readonly Mock<ILogger<InvoiceService>> _logger = new();
    private readonly Mock<IRandomProvider> _randomProvider = new();

    private IInvoiceService CreateService(
        string? memberKeyOverride = null,
        string? cookieValue = null)
    {
        // Set up the configuration to return the provided override value
        _configuration.Setup(c => c["Overrides:MemberKey"]).Returns(memberKeyOverride);

        // Set up the HttpContext with the cookie if provided
        var context = new DefaultHttpContext();
        if (!string.IsNullOrEmpty(cookieValue))
            context.Request.Headers["Cookie"] = $"memberKey={cookieValue}";

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(context);

        // Set up a mock environment (customize as needed)
        var env = new Mock<IWebHostEnvironment>();
        env.Setup(e => e.EnvironmentName).Returns("Development");

        // Use the real MemberKeyProvider
        var memberKeyProvider = new MemberKeyProvider(
            httpContextAccessor.Object,
            _configuration.Object,
            env.Object
        );

        // Setup predictable random values for PopulateDummyInvoices
        _randomProvider.Setup(r => r.Next(It.IsAny<int>(), It.IsAny<int>())).Returns(5);

        return new InvoiceService(
            _httpClientFactory.Object,
            _configuration.Object,
            _logger.Object,
            memberKeyProvider,
            _randomProvider.Object,
            0
        );
    }

    [Theory]
    [InlineData(MemberType.Chancery, 1200.50)]
    [InlineData(MemberType.SVC, 800.00)]
    [InlineData(MemberType.SIR, 500.00)]
    public async Task GetInvoiceSummaryAsync_ReturnsExpectedSummary(MemberType memberType, decimal expectedPriorBalance)
    {
        var service = CreateService();
        var result = await service.GetInvoiceSummaryAsync("any", memberType);

        Assert.NotNull(result);
        Assert.Equal(expectedPriorBalance, result.PriorBalance);
        Assert.NotNull(result.Details);
        Assert.NotEmpty(result.Details);
    }

    [Fact]
    public void PopulateDummyInvoices_PopulatesList()
    {
        var service = CreateService();
        var list = new List<InvoiceSummaryDetail>();

        service.PopulateDummyInvoices(list);

        Assert.NotEmpty(list);
        // With our mock, count will always be 5
        Assert.Equal(5, list.Count);
    }

    [Theory]
    [InlineData(MemberType.Chancery,"123", "0123-0000")]
    [InlineData(MemberType.SVC, "123", "0123-svc")]
    [InlineData(MemberType.SIR, "123", "0123-sir")]
    public void GetFormattedMemberKey_ReturnsCorrectFormat(MemberType memberType,string actual, string expected)
    {
        var service = CreateService(actual);
        var result = service.GetFormattedMemberKey(memberType);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(123.45, "$123.45")]
    [InlineData(null, "")]
    public void FormatCurrency_FormatsCorrectly(double? value, string expected)
    {
        var service = CreateService();
        decimal? decimalValue = value.HasValue ? (decimal?)value.Value : null;
        var result = service.FormatCurrency(decimalValue);
        Assert.Equal(expected, result);
    }
}