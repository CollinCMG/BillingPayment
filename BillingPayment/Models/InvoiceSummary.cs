
namespace BillingPayment.Models
{
    public class InvoiceSummary
    {
        public decimal PriorBalance { get; set; }
        public decimal ChargesAndFees { get; set; }
        public decimal PaymentsAndAdjustments { get; set; }
        public decimal AccountBalance { get; set; }
        public decimal MinimumDue { get; set; }
        public DateTime? DueDate { get; set; }
        public List<InvoiceSummaryDetail>? Details { get; set; }
    }
}
