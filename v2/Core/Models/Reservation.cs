using System.ComponentModel.Design.Serialization;

namespace v2.Core.Models;

public class Reservation
{
    public required int ID { get; set; }
    public required int UserID { get; set; }
    public required int ParkingLotID { get; set; }
    public required int VehicleID { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    public required string Status { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required float TotalPrice { get; set; }

    public User? User { get; set; }
    public ParkingLot? ParkingLot { get; set; }
    public Vehicle? Vehicle { get; set; }
}