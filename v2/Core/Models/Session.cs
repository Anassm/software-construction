namespace v2.Core.Models;

public class Session
{
     public required Guid ID { get; set; } = Guid.NewGuid();
    public required string LicensePlate { get; set; }
    public required DateTime StartTime { get; set; }
    public required PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    public required Guid UserID { get; set; }
    public required Guid ParkingLotID { get; set; }
    
    public DateTime? EndTime { get; set; }
    public int? DurationInMinutes { get; set; }
    public float? Price { get; set; }
    public Guid? PaymentID { get; set; }

    public User? User { get; set; }
    public ParkingLot? ParkingLot { get; set; }
    public Payment? Payment { get; set; }
}

public enum PaymentStatus
{
    Unpaid,
    Paid,
    Pending
}