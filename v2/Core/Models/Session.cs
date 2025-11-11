namespace v2.Core.Models;

public class Session
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public string OldID { get; set; } = "";
    public required string LicensePlate { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public int duration { get; set; }
    public float Price { get; set; } = 0;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    public Guid? UserID { get; set; }
    public Guid ParkingLotID { get; set; }
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