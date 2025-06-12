namespace BillingPayment.Models
{
    public class InvoiceSummaryDetail
    {
        public DateTime? TransactionDate { get; set; }
        public string? CertPolNo { get; set; }
        public string? Description { get; set; }
        public decimal TransactionAmount { get; set; }
        public decimal CreditsAndPaymentsApplied { get; set; }
        public decimal MinimumDue { get; set; }
    }
}
