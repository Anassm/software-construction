namespace v2.Core.DTOs;

public class ReservationCreateRequest
{
    public required string LicensePlate { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    public required Guid ParkingLotId { get; set; }
}

public class ReservationResponse
{
    public required Guid Id { get; set; }
    public required string LicensePlate { get; set; }
    public required Guid ParkingLotId { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    public required string Status { get; set; }
    public required float TotalPrice { get; set; }
    public required DateTime CreatedAt { get; set; }
}
