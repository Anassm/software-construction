namespace v2.Core.Models;

public class Vehicle
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public string OldID { get; set; } = "";
    public required string LicensePlate { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public string? Color { get; set; }
    public int? Year { get; set; }
    public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public required Guid UserID { get; set; }
    public User? User { get; set; }
    
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}