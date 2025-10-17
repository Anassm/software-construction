namespace v2.Core.Models;

public class Vehicle
{
    public required int ID { get; set; }
    public required int UserID { get; set; }
    public required string LicensePlate { get; set; }
    public required string Make { get; set; }
    public required string Model { get; set; }
    public required string Color { get; set; }
    public required int Year { get; set; }
    public required DateOnly CreatedAt { get; set; }

    public User? User { get; set; }
    public required ICollection<Reservation> Reservations { get; set; }
}