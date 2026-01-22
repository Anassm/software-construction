using Microsoft.AspNetCore.Mvc;
using v2.Infrastructure.Data;
using v2.Core.Models;
using v2.Core.DTOs;
using v2.Core.Interfaces;
using System.Security.Claims;

namespace v2.Controllers;

[ApiController]
[Route("parkinglots")]
public class ParkingLotsController : ControllerBase
{
     private readonly IParkingLots _parkingLotService;

    public ParkingLotsController(IParkingLots parkingLotService)
    {
        _parkingLotService = parkingLotService;
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ParkingLotCreateRequest dto)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);


        if (string.IsNullOrEmpty(identityUserId))
            return StatusCode(StatusCodes.Status401Unauthorized,
                new { error = "Unauthorized: Invalid or missing session token" });


        if (!ModelState.IsValid)
            return BadRequest(ModelState);

 
        var (statusCode, message) = await _parkingLotService.CreateParkingLotAsync(dto);

    
        return statusCode switch
        {
            201 => Created($"/parkinglots/{(message as dynamic).parkingLot.id}", message), 
            409 => Conflict(message),
            _ => StatusCode(statusCode, message)
        };

    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);


        if (string.IsNullOrEmpty(identityUserId))
            return StatusCode(StatusCodes.Status401Unauthorized,
                new { error = "Unauthorized: Invalid or missing session token" });


        var (statusCode, message) = await _parkingLotService.GetAllParkingLotsAsync();
        return StatusCode(statusCode, message); 
    }


    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);


        if (string.IsNullOrEmpty(identityUserId))
            return StatusCode(StatusCodes.Status401Unauthorized,
                new { error = "Unauthorized: Invalid or missing session token" });


        var (statusCode, message) = await _parkingLotService.GetParkingLotAsync(id);

        return statusCode switch
        {
            200 => Ok(message),
            404 => NotFound(message),
            _ => StatusCode(statusCode, message)
        };
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ParkingLotCreateRequest dto)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);


        if (string.IsNullOrEmpty(identityUserId))
            return StatusCode(StatusCodes.Status401Unauthorized,
                new { error = "Unauthorized: Invalid or missing session token" });


        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (statusCode, message) = await _parkingLotService.UpdateParkingLotAsync(id, dto);

        return statusCode switch
        {
            200 => Ok(message),
            404 => NotFound(message),
            409 => Conflict(message),
            _ => StatusCode(statusCode, message)
        };
    }


    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);


        if (string.IsNullOrEmpty(identityUserId))
            return StatusCode(StatusCodes.Status401Unauthorized,
                new { error = "Unauthorized: Invalid or missing session token" });


        var (statusCode, message) = await _parkingLotService.DeleteParkingLotAsync(id);

        return statusCode switch
        {
            200 => Ok(message),
            404 => NotFound(message),
            _ => StatusCode(statusCode, message)
        };
    }

  
    [HttpPost("{parkingLotId:guid}/sessions/start")]
    public async Task<IActionResult> StartSession(Guid parkingLotId, [FromBody] SessionStartRequest dto)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);


        if (string.IsNullOrEmpty(identityUserId))
            return StatusCode(StatusCodes.Status401Unauthorized,
                new { error = "Unauthorized: Invalid or missing session token" });


        var (statusCode, message) = await _parkingLotService.StartSessionAsync(parkingLotId, dto.LicensePlate, dto.UserId);
        return StatusCode(statusCode, message);
    }


    [HttpPost("{parkingLotId:guid}/sessions/stop")]
    public async Task<IActionResult> StopSession(Guid parkingLotId, [FromBody] SessionStopRequest dto)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);


        if (string.IsNullOrEmpty(identityUserId))
            return StatusCode(StatusCodes.Status401Unauthorized,
                new { error = "Unauthorized: Invalid or missing session token" });


      
        var (statusCode, message) = await _parkingLotService.StopSessionAsync(parkingLotId, dto.LicensePlate, dto.UserId);
        return StatusCode(statusCode, message);
    }

    [HttpGet("{parkingLotId:guid}/sessions")]
    public async Task<IActionResult> GetAllSessions(Guid parkingLotId)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);


        if (string.IsNullOrEmpty(identityUserId))
            return StatusCode(StatusCodes.Status401Unauthorized,
                new { error = "Unauthorized: Invalid or missing session token" });


        var (statusCode, message) = await _parkingLotService.GetAllSessionsForLotAsync(parkingLotId);
        return StatusCode(statusCode, message);
    }

 
    [HttpGet("{parkingLotId:guid}/sessions/{sessionId:guid}")]
    public async Task<IActionResult> GetSessionById(Guid parkingLotId, Guid sessionId)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);


        if (string.IsNullOrEmpty(identityUserId))
            return StatusCode(StatusCodes.Status401Unauthorized,
                new { error = "Unauthorized: Invalid or missing session token" });


        var (statusCode, message) = await _parkingLotService.GetSessionByIdAsync(parkingLotId, sessionId);
        return StatusCode(statusCode, message);
    }


    [HttpDelete("{parkingLotId:guid}/sessions/{sessionId:guid}")]
    public async Task<IActionResult> DeleteSession(Guid parkingLotId, Guid sessionId)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);


        if (string.IsNullOrEmpty(identityUserId))
            return StatusCode(StatusCodes.Status401Unauthorized,
                new { error = "Unauthorized: Invalid or missing session token" });


        var (statusCode, message) = await _parkingLotService.DeleteSessionAsync(parkingLotId, sessionId);
        return StatusCode(statusCode, message);
    }
}

