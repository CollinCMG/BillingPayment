namespace BillingPayment.Models;

public class LatestSummary
{
    public string? accountNo { get; set; }
    public string? sourceSystemRequestNo { get; set; }
    public decimal lastAssignedPaymentAmount { get; set; }
    public decimal lastInvoiceAmount { get; set; }
    public string? lastInvoiceDate { get; set; }
    public string? lastInvoiceDueDate { get; set; }
    public string? lastInvoiceDocumentPath { get; set; }
    public string? lastInvoiceOutputFileName { get; set; }
    public int? lastInvoiceOutputSequenceNo { get; set; }
    public decimal lastPaymentAmount { get; set; }
    public string? lastPaymentDate { get; set; }
    public string? lastPaymentId { get; set; }
    public decimal pleasePayAmount { get; set; }
    public decimal paymentInFull { get; set; }
}