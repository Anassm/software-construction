namespace v2.Core.DTOs;

public class ParkingLotCreateRequest
{
    public required string Name { get; set; }
    public required string Location { get; set; }
    public required string Address { get; set; }
    public required int Capacity { get; set; }
    public required float Tariff { get; set; }
    public required float DayTariff { get; set; }
    public required float Latitude { get; set; }
    public required float Longitude { get; set; }
}
