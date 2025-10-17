namespace v2.Core.Models;

public class Payment
{
    public int ID { get; set; }

    public required string Transaction { get; set; }
    public required float Amount { get; set; }
    public required string Initiator { get; set; }

    public required DateTime CreatedAt { get; set; }
    public required DateTime CompletedAt { get; set; }

    public required string Hash { get; set; }

    public required float TransactionAmount { get; set; }
    public required DateTime TransactionDate { get; set; }
    public required string Method { get; set; }
    public required string Issuer { get; set; }
    public required string Bank { get; set; }

    public required string SessionId { get; set; }
    public required string ParkingLotId { get; set; }
}
