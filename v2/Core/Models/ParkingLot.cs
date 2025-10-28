namespace v2.Core.Models;

public class ParkingLot
{
    public required Guid ID { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Location { get; set; }
    public required string Address { get; set; }
    public required int Capacity { get; set; }
    public required int Reserved { get; set; }
    public required float Tariff { get; set; }
    public required float DayTariff { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required float latitude { get; set; }
    public required float longitude { get; set; }

    public required ICollection<Reservation> Reservations { get; set; }
    public required ICollection<Session> Sessions { get; set; }
}