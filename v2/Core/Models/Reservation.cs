using System.ComponentModel.Design.Serialization;

namespace v2.Core.Models;

public class Reservation
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public string OldID { get; set; } = "";
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    public required string Status { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required float TotalPrice { get; set; }
    public string? CompanyName { get; set; }

    public required Guid UserID { get; set; }
    public required Guid ParkingLotID { get; set; }
    public required Guid VehicleID { get; set; }

    public User? User { get; set; }
    public ParkingLot? ParkingLot { get; set; }
    public Vehicle? Vehicle { get; set; }
}