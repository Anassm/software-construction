namespace v2.Core.DTOs;

using v2.Core.Models;

public class VehicleHistoryDTO
{
    public Vehicle Vehicle { get; set; } = default!;
    public IEnumerable<Reservation> Reservations { get; set; } = new List<Reservation>();
}

public class CreateVehicleDto
{
    public required string LicensePlate { get; set; }
    public string Make { get; set; } = "";
    public string Model { get; set; } = "";
    public string Color { get; set; } = "";
    public int Year { get; set; } = 0;
}

public class UpdateVehicleDto
{
    public string LicensePlate { get; set; } = "";
    public string Make { get; set; } = "";
    public string Model { get; set; } = "";
    public string Color { get; set; } = "";
    public int Year { get; set; } = 0;
    public Guid UserId { get; set; } = Guid.Empty;
}
