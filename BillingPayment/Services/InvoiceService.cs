using BillingPayment.Enums;
using BillingPayment.Models;
using BillingPayment.Providers;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BillingPayment.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InvoiceService> _logger;
        private readonly IMemberKeyProvider _memberKeyProvider;

        public InvoiceService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<InvoiceService> logger,
            IMemberKeyProvider memberKeyProvider
        )
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _memberKeyProvider = memberKeyProvider ?? throw new ArgumentNullException(nameof(memberKeyProvider));
        }

        public async Task<InvoiceSummary?> GetInvoiceSummaryAsync(string accountNo, MemberType memberType)
        {
            _logger.LogInformation("Fetching invoice summary for account {AccountNo} and member type {MemberType}", accountNo, memberType);

            try
            {
                var endpoint = _configuration["ExternalApi:BillingAccountsEndpoint"];
                var url = $"{endpoint}/billing-accounts/{accountNo}/latest-summary?sourceSystemUserId=test&sourceSystemCode=PAS";
                var response = await SendAuthenticatedGetRequestAsync(url);

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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching invoice summary for account {AccountNo} and member type {MemberType}", accountNo, memberType);
                throw;
            }
        }

        public async Task<List<MemberType>> GetAvailableMemberTypesAsync()
        {
            var results = new List<MemberType> { MemberType.Chancery };
            try
            {
                var tasks = new[]
                {
                    CheckPolicyInformationAsync(results, MemberType.SIR),
                    CheckPolicyInformationAsync(results, MemberType.SVC)
                };
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve available member types.");
                throw;
            }

            return results;
        }

        private async Task CheckPolicyInformationAsync(List<MemberType> results, MemberType memberType)
        {
            var accountNo = GetFormattedMemberKey(memberType);
            var endpoint = _configuration["ExternalApi:BillingAccountsEndpoint"];
            var url = $"{endpoint}/billing-accounts/{accountNo}/policy?sourceSystemUserId=test&sourceSystemCode=PAS";
            var response = await SendAuthenticatedGetRequestAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                // Check if "policies" property contains any elements
                var policyResponse = JsonSerializer.Deserialize<PolicyResponse>(result);
                if (policyResponse?.Policies != null && policyResponse.Policies.Count > 0)
                {
                    results.Add(memberType);
                }
            }
            else
            {
                _logger.LogError("API Error: {StatusCode} for account {AccountNo}", response.StatusCode, accountNo);
            }
        }

        private async Task<HttpResponseMessage> SendAuthenticatedGetRequestAsync(string url)
        {
            var username = _configuration["ExternalApi:Username"];
            var password = _configuration["ExternalApi:Password"];
            var client = _httpClientFactory.CreateClient();
            var byteArray = System.Text.Encoding.ASCII.GetBytes($"{username}:{password}");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            return await client.GetAsync(url);
        }

        public string GetFormattedMemberKey(MemberType selectedMemberType)
        {
            try
            {
                var memberKey = _memberKeyProvider.GetMemberKey();

                string suffix = selectedMemberType switch
                {
                    MemberType.Chancery => "-0000",
                    MemberType.SVC => "-SVC",
                    MemberType.SIR => "-SIR",
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

        private class PolicyResponse
        {
            [JsonPropertyName("policies")]
            public List<object>? Policies { get; set; }
        }
    }
}