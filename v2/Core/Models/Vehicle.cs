namespace v2.Core.Models;

public class Vehicle
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public string OldID { get; set; } = "";
    public required string LicensePlate { get; set; }
    public required string Make { get; set; }
    public required string Model { get; set; }
    public required string Color { get; set; }
    public required int Year { get; set; }
    public DateOnly CreatedAt { get; set; }
    public required Guid UserID { get; set; }
    public User? User { get; set; }
    
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}