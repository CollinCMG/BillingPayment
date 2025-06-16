using BillingPayment.Enums;
using BillingPayment.Interfaces;
using BillingPayment.Models;
using System.Globalization;

namespace BillingPayment.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InvoiceService> _logger;
        private readonly IMemberKeyProvider _memberKeyProvider;
        private readonly IRandomProvider _randomProvider;
        private readonly int _apiDelayMs;

        public InvoiceService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<InvoiceService> logger,
            IMemberKeyProvider memberKeyProvider,
            IRandomProvider randomProvider,
            int apiDelayMs = 0 // default to 0 for tests, set to 1000 in production if needed
        )
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _memberKeyProvider = memberKeyProvider ?? throw new ArgumentNullException(nameof(memberKeyProvider));
            _randomProvider = randomProvider ?? throw new ArgumentNullException(nameof(randomProvider));
            _apiDelayMs = apiDelayMs;
        }

        public async Task<InvoiceSummary?> GetInvoiceSummaryAsync(string accountNo, MemberType memberType)
        {
            _logger.LogInformation("Fetching invoice summary for account {AccountNo} and member type {MemberType}", accountNo, memberType);

            try
            {
                if (_apiDelayMs > 0)
                    await Task.Delay(_apiDelayMs);

                var summary = GetFakeInvoiceSummaryForMemberType(memberType);

                _logger.LogInformation("Successfully fetched invoice summary for account {AccountNo} and member type {MemberType}", accountNo, memberType);
                return summary;

                // Uncomment and use this block for real API calls
                /*
                var endpoint = _configuration["ExternalApi:BillingAccountsEndpoint"];
                var username = _configuration["ExternalApi:Username"];
                var password = _configuration["ExternalApi:Password"];
                var client = _httpClientFactory.CreateClient();
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{username}:{password}");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                var url = $"{endpoint}/your-route-here/{accountNo}";
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("API call successful for account {AccountNo}", accountNo);
                    return JsonSerializer.Deserialize<InvoiceSummary>(result);
                }
                else
                {
                    _logger.LogError("API Error: {StatusCode} for account {AccountNo}", response.StatusCode, accountNo);
                    throw new Exception($"API Error: {response.StatusCode}");
                }
                */
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching invoice summary for account {AccountNo} and member type {MemberType}", accountNo, memberType);
                throw;
            }
        }

        public void PopulateDummyInvoices(List<InvoiceSummaryDetail> list)
        {
            _logger.LogDebug("Populating dummy invoices for testing.");
            int count = _randomProvider.Next(1, 20);
            for (int i = 1; i <= count; i++)
            {
                list.Add(new InvoiceSummaryDetail
                {
                    TransactionDate = DateTime.Today.AddDays(-i),
                    CertPolNo = (i % 2 == 0) ? "8577" : "",
                    Description = (i % 2 == 0) ? "Prior Minimum Amount Due" : "Installment",
                    TransactionAmount = _randomProvider.Next(100000, 300000) / 100m,
                    CreditsAndPaymentsApplied = (i % 2 == 0) ? -_randomProvider.Next(100000, 150000) / 100m : -_randomProvider.Next(1000, 5000) / 100m,
                    MinimumDue = _randomProvider.Next(100000, 150000) / 100m,
                });
            }
            _logger.LogDebug("Dummy invoices populated: {Count}", list.Count);
        }

        private InvoiceSummary GetFakeInvoiceSummaryForMemberType(MemberType memberType)
        {
            _logger.LogDebug("Getting fake invoice summary for member type {MemberType}", memberType);
            return memberType switch
            {
                MemberType.Chancery => new InvoiceSummary
                {
                    PriorBalance = 1200.50m,
                    ChargesAndFees = 350.75m,
                    PaymentsAndAdjustments = -200.00m,
                    AccountBalance = 1351.25m,
                    MinimumDue = 100.00m,
                    DueDate = DateTime.Today.AddDays(10),
                    InvoiceNumber = "INV-1234",
                    Details = new List<InvoiceSummaryDetail>
                    {
                        new InvoiceSummaryDetail
                        {
                            TransactionDate = DateTime.Today.AddDays(-1),
                            CertPolNo = "CH-1001",
                            Description = "Chancery Installment",
                            TransactionAmount = 500.00m,
                            CreditsAndPaymentsApplied = -100.00m,
                            MinimumDue = 100.00m
                        }
                    }
                },
                MemberType.SVC => new InvoiceSummary
                {
                    PriorBalance = 800.00m,
                    ChargesAndFees = 150.00m,
                    PaymentsAndAdjustments = -50.00m,
                    AccountBalance = 900.00m,
                    MinimumDue = 75.00m,
                    DueDate = DateTime.Today.AddDays(15),
                    Details = new List<InvoiceSummaryDetail>
                    {
                        new InvoiceSummaryDetail
                        {
                            TransactionDate = DateTime.Today.AddDays(-2),
                            CertPolNo = "SVC-2002",
                            Description = "SVC Service Fee",
                            TransactionAmount = 200.00m,
                            CreditsAndPaymentsApplied = -25.00m,
                            MinimumDue = 75.00m
                        }
                    }
                },
                MemberType.SIR => new InvoiceSummary
                {
                    PriorBalance = 500.00m,
                    ChargesAndFees = 100.00m,
                    PaymentsAndAdjustments = -20.00m,
                    AccountBalance = 580.00m,
                    MinimumDue = 50.00m,
                    DueDate = DateTime.Today.AddDays(20),
                    Details = new List<InvoiceSummaryDetail>
                    {
                        new InvoiceSummaryDetail
                        {
                            TransactionDate = DateTime.Today.AddDays(-3),
                            CertPolNo = "SIR-3003",
                            Description = "SIR Risk Fee",
                            TransactionAmount = 100.00m,
                            CreditsAndPaymentsApplied = -10.00m,
                            MinimumDue = 50.00m
                        }
                    }
                },
                _ => new InvoiceSummary
                {
                    PriorBalance = 0m,
                    ChargesAndFees = 0m,
                    PaymentsAndAdjustments = 0m,
                    AccountBalance = 0m,
                    MinimumDue = 0m,
                    DueDate = null,
                    Details = new List<InvoiceSummaryDetail>()
                }
            };
        }

        public string GetFormattedMemberKey(MemberType selectedMemberType)
        {
            try
            {
                var memberKey = _memberKeyProvider.GetMemberKey();

                string suffix = selectedMemberType switch
                {
                    MemberType.Chancery => "-0000",
                    MemberType.SVC => "-svc",
                    MemberType.SIR => "-sir",
                    _ => "-0000"
                };

                var formattedKey = memberKey + suffix;
                _logger.LogInformation("Formatted member key: {FormattedKey} for member type {MemberType}", formattedKey, selectedMemberType);
                return formattedKey;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error formatting member key for member type {MemberType}", selectedMemberType);
                throw;
            }
        }

        public string FormatCurrency(decimal? value)
        {
            return value.HasValue ? value.Value.ToString("C", CultureInfo.CurrentCulture) : "";
        }
    }
}