namespace v2.Core.DTOs;

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
