namespace v2.Core.Models;

public class Payment
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public string OldID { get; set; } = "";
    public float Amount { get; set; } = 0;
    public required string Initiator { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; } = null;
    public string? Hash { get; set; }

    public float TransactionAmount { get; set; } = 0;
    public required DateTime TransactionDate { get; set; }
    public required string TransactionMethod { get; set; }
    public required string TransactionIssuer { get; set; }
    public required string TransactionBank { get; set; }    

    public required Guid SessionID { get; set; }
    public Session? Session { get; set; }
}