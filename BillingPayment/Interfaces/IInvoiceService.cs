using BillingPayment.Enums;
using BillingPayment.Models;

namespace BillingPayment.Interfaces
{
    public interface IInvoiceService
    {
        Task<InvoiceSummary?> GetInvoiceSummaryAsync(string accountNo, MemberType memberType);
        void PopulateDummyInvoices(List<InvoiceSummaryDetail> list);
        string GetFormattedMemberKey(MemberType selectedMemberType);
        string FormatCurrency(decimal? value);
    }
}
