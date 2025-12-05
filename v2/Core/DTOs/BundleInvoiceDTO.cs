namespace v2.Core.DTOs;

public class CreateBundleInvoiceDto
{
    public required List<Guid> SessionIds { get; set; }
    public string? CompanyName { get; set; }
    public DateTime? DueDate { get; set; }
}

public class BundleInvoiceResponseDto
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; }
    public float TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; }
    public int SessionCount { get; set; }
    public List<Guid> BundledSessionIds { get; set; }
    public string? CompanyName { get; set; }
}