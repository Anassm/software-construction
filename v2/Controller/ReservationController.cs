using Microsoft.AspNetCore.Mvc;
using v2.core.Interfaces;
using v2.Core.DTOs;
using System.Security.Claims;
using System;

namespace v2.Controllers;

[ApiController]
[Route("reservations")]
public class ReservationController : ControllerBase
{
    private readonly IReservation _reservationService;
    public ReservationController(IReservation reservationService) => _reservationService = reservationService;

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID claim not found or invalid."); 
        }
        return userId;
    }

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

    /// <summary>Retrieves all reservations for the authenticated user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetUserReservations()
    {
        if (!User.Identity?.IsAuthenticated ?? true) 
        {
             return Unauthorized(); 
        }

        try
        {
            var userId = GetUserId();
            var reservations = await _reservationService.GetReservationsAsync(userId);
            
            var responseList = reservations.Select(r => new ReservationResponse 
            {
                Id = r.ID,
                LicensePlate = r.Vehicle?.LicensePlate ?? "Unknown", 
                ParkingLotId = r.ParkingLotID,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                Status = r.Status,
                TotalPrice = r.TotalPrice,
                CreatedAt = r.CreatedAt
            }).ToList();

            return Ok(responseList);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "An internal error occurred while retrieving reservations." });
        }
    }
}