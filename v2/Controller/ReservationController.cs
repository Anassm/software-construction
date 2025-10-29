using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using v2.Core.Interfaces;
using v2.Core.DTOs;

namespace v2.Controllers;

[ApiController]
[Route("reservations")]
[Authorize]
public class ReservationController : ControllerBase
{
    private readonly IReservation _reservationService;
    private readonly ILogger<ReservationController> _logger;
    
    public ReservationController(IReservation reservationService, ILogger<ReservationController> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }

    /// <summary>Create a reservation by license plate, dates and parking lot.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReservationCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid request body for reservation creation");
            return BadRequest(new { error = "Invalid request body." });
        }

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
            _logger.LogWarning(ex, "Validation error creating reservation");
            return BadRequest(new { error = ex.Message });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error creating reservation");
            return StatusCode(500, new { error = "An error occurred while creating the reservation." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating reservation");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
