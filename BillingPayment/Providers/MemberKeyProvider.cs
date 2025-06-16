using BillingPayment.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace BillingPayment.Providers
{
    public class MemberKeyProvider : IMemberKeyProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public MemberKeyProvider(
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IWebHostEnvironment env)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public string GetMemberKey()
        {
            var memberKey = _httpContextAccessor.HttpContext?.Request.Cookies["memberKey"];
            if (string.IsNullOrWhiteSpace(memberKey) && _env.IsDevelopment())
            {
                memberKey = _configuration["Overrides:MemberKey"];
            }
            if (string.IsNullOrWhiteSpace(memberKey))
            {
                throw new InvalidOperationException("Member key is required but was not found in cookies or development settings.");
            }
            return memberKey.PadLeft(4, '0');
        }
    }
}