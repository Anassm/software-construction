using Microsoft.AspNetCore.Mvc;
using v2.core.Interfaces;
using v2.Core.DTOs;

namespace v2.Controllers;

[ApiController]
[Route("reservations")]
public class ReservationController : ControllerBase
{
    private readonly IReservation _reservationService;
    public ReservationController(IReservation reservationService) => _reservationService = reservationService;

    /// <summary>Create a reservation by license plate, dates and parking lot.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReservationCreateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { error = "Invalid request body." });

        try
        {
            var created = await _reservationService.CreateReservationAsync(request);

            var response = new ReservationResponse
            {
                Id = created.ID,
                LicensePlate = request.LicensePlate,
                ParkingLotId = created.ParkingLotID,
                StartDate = created.StartDate,
                EndDate = created.EndDate,
                Status = created.Status,
                TotalPrice = created.TotalPrice,
                CreatedAt = created.CreatedAt
            };

            return Created($"/reservations/{response.Id}", response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
