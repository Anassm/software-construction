using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using v2.core.Interfaces;
using v2.Core.DTOs;

namespace v2.Controllers;

[ApiController]
[Route("reservations")]
public class ReservationController : ControllerBase
{
    private readonly IReservation _reservationService;
    public ReservationController(IReservation reservationService) => _reservationService = reservationService;

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
                Id          = created.ID,
                LicensePlate = request.LicensePlate,
                VehicleId   = created.VehicleID,
                ParkingLotId = created.ParkingLotID,
                StartDate   = created.StartDate,
                EndDate     = created.EndDate,
                Status      = created.Status,
                TotalPrice  = created.TotalPrice,
                CreatedAt   = created.CreatedAt
            };

            return Created($"/reservations/{response.Id}", response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetForCurrentUser()
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (identityUserId == null)
            return StatusCode(StatusCodes.Status401Unauthorized,
                new { error = "Unauthorized: Invalid or missing session token" });

        try
        {
            var reservations = await _reservationService.GetReservationsForUserAsync(identityUserId);

            var result = reservations.Select(r => new ReservationResponse
            {
                Id          = r.ID,
                LicensePlate = r.Vehicle?.LicensePlate ?? string.Empty,
                VehicleId   = r.VehicleID,
                ParkingLotId = r.ParkingLotID,
                StartDate   = r.StartDate,
                EndDate     = r.EndDate,
                Status      = r.Status,
                TotalPrice  = r.TotalPrice,
                CreatedAt   = r.CreatedAt
            });

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (identityUserId == null)
            return StatusCode(StatusCodes.Status401Unauthorized,
                new { error = "Unauthorized: Invalid or missing session token" });

        var success = await _reservationService.DeleteReservationForUserAsync(id, identityUserId);

        if (!success)
            return NotFound(new { error = "Reservation not found or not owned by current user." });

        return NoContent();
    }
}
