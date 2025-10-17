namespace v2.Core.Models;

public class Session
{
    public required int ID { get; set; }
    public required int ParkingLotID { get; set; }
    public required string LicensePlate { get; set; }
    public required DateTime StartTime { get; set; }
    public required DateTime? EndTime { get; set; }
    public required int UserID { get; set; }
    public required int duration { get; set; }
    public required float Price { get; set; } = 0;
    public required PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
}

public enum PaymentStatus
{
    Unpaid,
    Paid,
    Pending
}