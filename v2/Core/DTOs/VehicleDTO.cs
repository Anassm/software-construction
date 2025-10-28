namespace v2.Core.DTOs;
using v2.Core.Models;
public class VehicleHistoryDTO
{
    public Vehicle Vehicle { get; set; } = default!;
    public IEnumerable<Reservation> Reservations { get; set; } = new List<Reservation>();
}

public class CreateVehicleDto
{
    public string LicensePlate { get; set; } = default!;
    public string Make { get; set; } = default!;
    public string Model { get; set; } = default!;
    public string Color { get; set; } = default!;
    public int Year { get; set; }
    public Guid UserID { get; set; }
}
