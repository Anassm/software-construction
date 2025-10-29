namespace v2.Core.Models;

public class Payment
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public required decimal Amount { get; set; }
    public required string Initiator { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Hash { get; set; }

    public required decimal TransactionAmount { get; set; }
    public required DateTime TransactionDate { get; set; }
    public required string TransactionMethod { get; set; }
    public required string TransactionIssuer { get; set; }
    public required string TransactionBank { get; set; }

    public required Guid SessionID { get; set; }

    public Session? Session { get; set; }
}