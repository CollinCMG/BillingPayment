
using System.Text.Json.Serialization;

namespace BillingPayment.Models
{
    public class InvoiceSummary
    {
        [JsonPropertyName("lastAssignedPaymentAmount")]
        public decimal PreviousPayAmount { get; set; }

        [JsonPropertyName("paymentInFull")]
        public decimal AccountBalance { get; set; }

        [JsonPropertyName("pleasePayAmount")]
        public decimal MinimumBalance { get; set; }

        [JsonPropertyName("lastInvoiceDueDate")]
        public DateTime? DueDate { get; set; }
    }
}
