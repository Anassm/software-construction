namespace v2.Core.Models;

public class Invoice
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public string InvoiceNumber { get; set; } = string.Empty;
    public float TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Open;

    public Guid UserID { get; set; }
    public User User { get; set; } = null!;

    
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}

public enum InvoiceStatus
{
    Open,
    Paid,
    Overdue,
    Void
}