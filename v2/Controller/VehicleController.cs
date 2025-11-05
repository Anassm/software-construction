using v2.Core.DTOs;
using v2.Core.Models;
using v2.core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using System.Security.Claims;


namespace v2.Controller;

[ApiController]
[Route("vehicles")]
public class VehicleController : ControllerBase
{
    private readonly IVehicles _vehicleService;

    public VehicleController(IVehicles vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleDto dto)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (identityUserId == null)
            return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

        if (dto == null)
            return StatusCode(StatusCodes.Status400BadRequest, new { error = "Request must contain a body." });

        if (dto.LicensePlate == null || dto.LicensePlate == string.Empty)
            return StatusCode(StatusCodes.Status400BadRequest, new { error = "Required field missing, field: LicensePlate" });

        dto.LicensePlate = dto.LicensePlate.Replace("-", "");
        var result = await _vehicleService.CreateVehicleAsync(dto, identityUserId);

        return result.statusCode switch
        {
            201 => StatusCode(StatusCodes.Status201Created, result.message),
            404 => StatusCode(StatusCodes.Status404NotFound, result.message),
            409 => StatusCode(StatusCodes.Status409Conflict, result.message),
            500 => StatusCode(StatusCodes.Status500InternalServerError, result.message),
            _ => StatusCode(StatusCodes.Status501NotImplemented, new { error = $"Unhandled statuscode: {result.statusCode}" })
        };
    }

    // [HttpPost("{lid}/entry")]
    // public async Task<IActionResult> StartSessionByEntry(string lid, [FromBody] UpdateVehicleEntryDto dto)
    // {
    //     ...
    // }

    [HttpPut("{lid}")]
    public async Task<IActionResult> UpdateVehicle(string lid, [FromBody] UpdateVehicleDto dto)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (identityUserId == null)
            return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

        if (lid == null || lid == string.Empty)
            return StatusCode(StatusCodes.Status400BadRequest, new { error = "Required route parameter missing, parameter: lid" });

        if (dto == null)
            return StatusCode(StatusCodes.Status400BadRequest, new { error = "Request must contain a body." });

        if (dto.LicensePlate == null || dto.LicensePlate == string.Empty)
            return StatusCode(StatusCodes.Status400BadRequest, new { error = "Require field missing, field: LicensePlate" });

        var result = await _vehicleService.UpdateVehicleAsync(lid, dto, identityUserId);

        return result.statusCode switch
        {
            200 => StatusCode(StatusCodes.Status200OK, result.message),
            404 => StatusCode(StatusCodes.Status404NotFound, result.message),
            409 => StatusCode(StatusCodes.Status409Conflict, result.message),
            500 => StatusCode(StatusCodes.Status500InternalServerError, result.message),
            _ => StatusCode(StatusCodes.Status501NotImplemented, new { error = $"Unhandled statuscode: {result.statusCode}" })
        };
    }

    [HttpDelete("{lid}")]
    public async Task<IActionResult> DeleteVehicle(string lid)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (identityUserId == null)
            return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

        if (lid == null || lid == string.Empty)
            return StatusCode(StatusCodes.Status400BadRequest, new { error = "Required route parameter missing, parameter: lid" });

        var result = await _vehicleService.DeleteVehicleAsync(lid, identityUserId);

        return result.statusCode switch
        {
            200 => StatusCode(StatusCodes.Status200OK, result.message),
            404 => StatusCode(StatusCodes.Status404NotFound, result.message),
            500 => StatusCode(StatusCodes.Status500InternalServerError, result.message),
            _ => StatusCode(StatusCodes.Status501NotImplemented, new { error = $"Unhandled statuscode: {result.statusCode}" })
        };
    }

    [HttpGet]
    public async Task<IActionResult> GetAllVehicles()
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (identityUserId == null)
            return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

        var result = await _vehicleService.GetAllVehiclesAsync(identityUserId);

        return result.statusCode switch
        {
            200 => StatusCode(StatusCodes.Status200OK, result.data),
            404 => StatusCode(StatusCodes.Status404NotFound, result.message),
            500 => StatusCode(StatusCodes.Status500InternalServerError, result.message),
            _ => StatusCode(StatusCodes.Status501NotImplemented, new { error = $"Unhandled statuscode: {result.statusCode}" })
        };
    }

    [HttpGet("{username}")]
    public async Task<IActionResult> GetAllVehiclesforUser([FromQuery] string? username)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (identityUserId == null)
            return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

        var result = await _vehicleService.GetAllVehiclesForUserAsync(username, identityUserId);

        return result.statusCode switch
        {
            200 => StatusCode(StatusCodes.Status200OK, result.data),
            404 => StatusCode(StatusCodes.Status404NotFound, result.message),
            500 => StatusCode(StatusCodes.Status500InternalServerError, result.message),
            _ => StatusCode(StatusCodes.Status501NotImplemented, new { error = $"Unhandled statuscode: {result.statusCode}" })
        };
    }

    [HttpGet("{vid}/reservations")]
    public async Task<IActionResult> GetReservationsByVehicle([FromRoute] string vid)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (identityUserId == null)
            return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

        var result = await _vehicleService.GetReservationsByVehicleAsync(vid, identityUserId);

        return result.statusCode switch
        {
            200 => StatusCode(StatusCodes.Status200OK, result.data),
            404 => StatusCode(StatusCodes.Status404NotFound, result.message),
            500 => StatusCode(StatusCodes.Status500InternalServerError, result.message),
            _ => StatusCode(StatusCodes.Status501NotImplemented, new { error = $"Unhandled statuscode: {result.statusCode}" })
        };
    }

    [HttpGet("{vid}/history")]
    public async Task<IActionResult> GetVehicleHistory([FromRoute] string vid)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (identityUserId == null)
            return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

        var result = await _vehicleService.GetVehicleHistoryAsync(vid, identityUserId);

        return result.statusCode switch
        {
            200 => StatusCode(StatusCodes.Status200OK, result.data),
            404 => StatusCode(StatusCodes.Status404NotFound, result.message),
            500 => StatusCode(StatusCodes.Status500InternalServerError, result.message),
            _ => StatusCode(StatusCodes.Status501NotImplemented, new { error = $"Unhandled statuscode: {result.statusCode}" })
        };
    }

    [HttpGet("health")]
    public async Task<IActionResult> health()
    {
        return Ok(); 
    }
}