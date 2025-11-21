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
                Id           = created.ID,
                LicensePlate = request.LicensePlate,
                VehicleId    = created.VehicleID,
                ParkingLotId = created.ParkingLotID,
                StartDate    = created.StartDate,
                EndDate      = created.EndDate,
                Status       = created.Status,
                TotalPrice   = created.TotalPrice,
                DiscountId   = created.DiscountID,
                CreatedAt    = created.CreatedAt
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
                Id           = r.ID,
                LicensePlate = r.Vehicle?.LicensePlate ?? string.Empty,
                VehicleId    = r.VehicleID,
                ParkingLotId = r.ParkingLotID,
                StartDate    = r.StartDate,
                EndDate      = r.EndDate,
                Status       = r.Status,
                TotalPrice   = r.TotalPrice,
                DiscountId   = r.DiscountID,
                CreatedAt    = r.CreatedAt
            });

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ReservationUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { error = "Invalid request body." });

        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (identityUserId == null)
            return StatusCode(StatusCodes.Status401Unauthorized,
                new { error = "Unauthorized: Invalid or missing session token" });

        var result = await _reservationService.UpdateReservationForUserAsync(id, identityUserId, request);

        return result.statusCode switch
        {
            200 when result.reservation != null => Ok(new ReservationResponse
            {
                Id           = result.reservation.ID,
                LicensePlate = result.reservation.Vehicle?.LicensePlate ?? string.Empty,
                VehicleId    = result.reservation.VehicleID,
                ParkingLotId = result.reservation.ParkingLotID,
                StartDate    = result.reservation.StartDate,
                EndDate      = result.reservation.EndDate,
                Status       = result.reservation.Status,
                TotalPrice   = result.reservation.TotalPrice,
                DiscountId   = result.reservation.DiscountID,
                CreatedAt    = result.reservation.CreatedAt
            }),
            400 => StatusCode(StatusCodes.Status400BadRequest, result.message),
            404 => StatusCode(StatusCodes.Status404NotFound, result.message),
            500 => StatusCode(StatusCodes.Status500InternalServerError, result.message),
            _   => StatusCode(StatusCodes.Status501NotImplemented,
                    new { error = $"Unhandled statuscode: {result.statusCode}" })
        };
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
