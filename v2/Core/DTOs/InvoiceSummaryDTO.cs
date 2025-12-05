public class InvoiceSummaryDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; }
    public float TotalAmount { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; }
}