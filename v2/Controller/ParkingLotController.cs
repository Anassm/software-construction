using Microsoft.AspNetCore.Mvc;
using ChefServe.Infrastructure.Data;
using v2.Core.Models;
using v2.Core.DTOs;
using v2.Core.Interfaces;

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
        var (statusCode, message) = await _parkingLotService.GetAllParkingLotsAsync();
        return StatusCode(statusCode, message); 
    }


    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
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
        var (statusCode, message) = await _parkingLotService.DeleteParkingLotAsync(id);

        return statusCode switch
        {
            200 => Ok(message),
            404 => NotFound(message),
            _ => StatusCode(statusCode, message)
        };
    }
}

