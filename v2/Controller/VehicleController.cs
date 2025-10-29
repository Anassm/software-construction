using Microsoft.AspNetCore.Mvc;
using v2.Core.Interfaces;
using v2.Core.DTOs;

namespace v2.Controllers;

[ApiController]
[Route("api/vehicles")]
public class VehicleController : ControllerBase
{
    private readonly IVehicles _vehicleService;

    public VehicleController(IVehicles vehicleService)
    {
        _vehicleService = vehicleService;
    }

    /// <summary>Create a new vehicle</summary>
    [HttpPost]
    public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleDto vehicle)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdVehicle = await _vehicleService.CreateVehicleAsync(vehicle);
            return CreatedAtAction(nameof(GetVehicleHistory), new { licensePlate = createdVehicle.LicensePlate }, createdVehicle);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Update an existing vehicle by license plate</summary>
    [HttpPut("{licensePlate}")]
    public async Task<IActionResult> UpdateVehicle(string licensePlate, [FromBody] CreateVehicleDto updatedVehicle)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var vehicle = await _vehicleService.UpdateVehicleAsync(licensePlate, updatedVehicle);
            
            if (vehicle == null)
            {
                return NotFound(new { error = $"Vehicle with license plate {licensePlate} not found." });
            }

            return Ok(vehicle);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Delete a vehicle by license plate</summary>
    [HttpDelete("{licensePlate}")]
    public async Task<IActionResult> DeleteVehicle(string licensePlate)
    {
        try
        {
            var deleted = await _vehicleService.DeleteVehicleAsync(licensePlate);
            
            if (!deleted)
            {
                return NotFound(new { error = $"Vehicle with license plate {licensePlate} not found." });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Get all vehicles, optionally filtered by user ID</summary>
    [HttpGet]
    public async Task<IActionResult> GetAllVehicles([FromQuery] Guid? userId = null)
    {
        try
        {
            var vehicles = await _vehicleService.GetAllVehiclesAsync(userId);
            return Ok(vehicles);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Get reservations for a specific vehicle by license plate</summary>
    [HttpGet("{licensePlate}/reservations")]
    public async Task<IActionResult> GetReservationsByVehicle(string licensePlate)
    {
        try
        {
            var reservations = await _vehicleService.GetReservationsByVehicleAsync(licensePlate);
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Get vehicle history including all reservations</summary>
    [HttpGet("{licensePlate}/history")]
    public async Task<IActionResult> GetVehicleHistory(string licensePlate)
    {
        try
        {
            var history = await _vehicleService.GetVehicleHistoryAsync(licensePlate);
            
            if (history == null)
            {
                return NotFound(new { error = $"Vehicle with license plate {licensePlate} not found." });
            }

            return Ok(history);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
