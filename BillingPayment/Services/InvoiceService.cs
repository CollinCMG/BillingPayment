using BillingPayment.Enums;
using BillingPayment.Models;
using System.Globalization;

namespace BillingPayment.Services
{
    public class InvoiceService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public InvoiceService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<InvoiceSummary?> GetInvoiceSummaryAsync(string accountNo, MemberType memberType)
        {
            // Simulate API latency
            await Task.Delay(1000);
            return GetFakeInvoiceSummaryForMemberType(memberType);

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
                return JsonSerializer.Deserialize<InvoiceSummary>(result);
            }
            else
            {
                throw new Exception($"API Error: {response.StatusCode}");
            }
            */
        }

        public void PopulateDummyInvoices(List<InvoiceSummaryDetail> list)
        {
            var random = new Random();
            for (int i = 1; i <= random.Next(1, 20); i++)
            {
                list.Add(new InvoiceSummaryDetail
                {
                    TransactionDate = DateTime.Today.AddDays(-i),
                    CertPolNo = (i % 2 == 0) ? "8577" : "",
                    Description = (i % 2 == 0) ? "Prior Minimum Amount Due" : "Installment",
                    TransactionAmount = random.Next(100000, 300000) / 100m,
                    CreditsAndPaymentsApplied = (i % 2 == 0) ? -random.Next(100000, 150000) / 100m : -random.Next(1000, 5000) / 100m,
                    MinimumDue = random.Next(100000, 150000) / 100m,
                });
            }
        }

        private InvoiceSummary GetFakeInvoiceSummaryForMemberType(MemberType memberType)
        {
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

        public string GetFormattedMemberKey(
            MemberType selectedMemberType,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IWebHostEnvironment env)
        {
            // Get Member Key from cookie or config
            var memberKey = httpContextAccessor.HttpContext?.Request.Cookies["memberKey"];
            if (string.IsNullOrWhiteSpace(memberKey) && env.IsDevelopment())
            {
                memberKey = configuration["Overrides:MemberKey"];
            }
            if (string.IsNullOrWhiteSpace(memberKey))
            {
                throw new InvalidOperationException("Member key is required but was not found in cookies or development settings.");
            }

            // Pad to 4 digits
            memberKey = memberKey.PadLeft(4, '0');

            // Append suffix based on MemberType
            string suffix = selectedMemberType switch
            {
                MemberType.Chancery => "-0000",
                MemberType.SVC => "-svc",
                MemberType.SIR => "-sir",
                _ => "-0000"
            };

            return memberKey + suffix;
        }

        public string FormatCurrency(decimal? value)
        {
            return value.HasValue ? value.Value.ToString("C", CultureInfo.CurrentCulture) : "";
        }
    }
}