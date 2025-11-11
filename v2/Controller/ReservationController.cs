using Microsoft.AspNetCore.Mvc;
using v2.core.Interfaces;
using v2.Core.DTOs;
using System.Security.Claims;

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

    /// <summary>Get all reservations for the authenticated user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetForCurrentUser()
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (identityUserId == null)
            return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

        try
        {
            var reservations = await _reservationService.GetReservationsForUserAsync(identityUserId);

            var response = reservations.Select(r => new ReservationResponse
            {
                Id = r.ID,
                LicensePlate = r.Vehicle?.LicensePlate ?? "",
                ParkingLotId = r.ParkingLotID,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                Status = r.Status,
                TotalPrice = r.TotalPrice,
                CreatedAt = r.CreatedAt
            });

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>Delete a reservation of the authenticated user.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (identityUserId == null)
            return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

        try
        {
            var deleted = await _reservationService.DeleteReservationForUserAsync(id, identityUserId);

            if (!deleted)
            {
                return NotFound(new { error = "Reservation not found or does not belong to the authenticated user." });
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred." });
        }
    }
}
