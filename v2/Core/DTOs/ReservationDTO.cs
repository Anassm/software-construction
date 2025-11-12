namespace v2.Core.DTOs;

public class ReservationCreateRequest
{
    public string LicensePlate { get; set; } = string.Empty;
    public Guid? VehicleId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid ParkingLotId { get; set; }
}

public class ReservationResponse
{
    public Guid Id { get; set; }

    public string LicensePlate { get; set; } = string.Empty;
    public Guid VehicleId { get; set; }
    public Guid ParkingLotId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public string Status { get; set; } = string.Empty;
    public float TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
}
