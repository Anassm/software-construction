namespace v2.Core.Models;

public class Payment
{
    public Guid ID { get; set; }
    public required float Amount { get; set; }
    public required string Initiator { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime CompletedAt { get; set; }
    public string? Hash { get; set; }

    public required float TransactionAmount { get; set; }
    public required DateTime TransactionDate { get; set; }
    public required string TransactionMethod { get; set; }
    public required string TransactionIssuer { get; set; }
    public required string TransactionBank { get; set; }

    public required Guid SessionID { get; set; }

    public required Session Session { get; set; }
}