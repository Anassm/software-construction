using Microsoft.AspNetCore.Mvc;
using v2.Infrastructure.Data;
using v2.Core.Models;
using v2.Core.DTOs;

namespace v2.Controllers;

[ApiController]
[Route("parkinglots")]
public class ParkingLotsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public ParkingLotsController(ApplicationDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ParkingLotCreateRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var lot = new ParkingLot
        {
            ID = Guid.NewGuid(),
            Name = dto.Name,
            Location = dto.Location,
            Address = dto.Address,
            Capacity = dto.Capacity,
            Reserved = 0,
            Tariff = dto.Tariff,
            DayTariff = dto.DayTariff,
            CreatedAt = DateTime.UtcNow,
            latitude = dto.Latitude,
            longitude = dto.Longitude,
            Reservations = new List<Reservation>(),
            Sessions = new List<Session>()
        };

        _db.ParkingLots.Add(lot);
        await _db.SaveChangesAsync();

        return Created($"/parkinglots/{lot.ID}", new {
            id = lot.ID,
            lot.Name,
            lot.Location,
            lot.Address,
            lot.Capacity,
            lot.Tariff,
            lot.DayTariff,
            lot.latitude,
            lot.longitude
        });
    }
}
