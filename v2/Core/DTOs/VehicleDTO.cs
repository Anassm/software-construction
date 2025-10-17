namespace v2.Core.DTOs;

public class RegisterDto
{
    public required string LicensePlate { get; set; }
    public required string Make { get; set; }
    public required string Model { get; set; }
    public required string Color { get; set; }
    public required int Year { get; set; }
}