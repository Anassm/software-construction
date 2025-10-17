using Microsoft.AspNetCore.Mvc;
using v2.Core.DTOs;

[ApiController]
[Route("api/[controller]")]
public class VehicleController : ControllerBase
{
    public VehicleController() { }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        return Ok(new { message = "Vehicle successfully made." });
    }
}