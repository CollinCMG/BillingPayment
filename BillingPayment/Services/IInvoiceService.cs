using BillingPayment.Enums;
using BillingPayment.Models;

namespace BillingPayment.Services
{
    public interface IInvoiceService
    {
        Task<InvoiceSummary?> GetInvoiceSummaryAsync(string accountNo, MemberType memberType);
        Task<List<MemberType>> GetAvailableMemberTypesAsync();
        string GetFormattedMemberKey(MemberType selectedMemberType);
        string FormatCurrency(decimal? value);
    }
}
