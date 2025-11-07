using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using v2.Core.DTOs;
using v2.Core.Models;
using System.Threading.Tasks;
using v2.Infrastructure.Data;
using v2.core.Interfaces;
using v2.infrastructure.Services;

namespace v2.Controller;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ApplicationDbContext dbContext)
    {
        
        _authService = new AuthService(dbContext, signInManager, userManager);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterUser(dto);

        return result.statusCode switch
        {
            400 => StatusCode(StatusCodes.Status400BadRequest, result.message),
            201 => StatusCode(StatusCodes.Status201Created, result.message),
            404 => StatusCode(StatusCodes.Status404NotFound, result.message),
            500 => StatusCode(StatusCodes.Status500InternalServerError, result.message),
            _ => StatusCode(StatusCodes.Status501NotImplemented, new { error = $"Unhandled statuscode: {result.statusCode}" })
        };
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginUser(dto);

        return result.statusCode switch
        {
            400 => StatusCode(StatusCodes.Status400BadRequest, result.message),
            401 => StatusCode(StatusCodes.Status401Unauthorized, result.message),
            200 => StatusCode(StatusCodes.Status200OK, result.message),
            404 => StatusCode(StatusCodes.Status404NotFound, result.message),
            500 => StatusCode(StatusCodes.Status500InternalServerError, result.message),
            _ => StatusCode(StatusCodes.Status501NotImplemented, new { error = $"Unhandled statuscode: {result.statusCode}" })
        };
    }

    [HttpPut("profile")]
    public async Task<IActionResult> Profile([FromBody] ProfileDto dto)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (identityUserId == null)
            return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

        var result = await _authService.UpdateProfile(dto, identityUserId);
        
        return result.statusCode switch
        {
            200 => StatusCode(StatusCodes.Status200OK, result.message),
            404 => StatusCode(StatusCodes.Status404NotFound, result.message),
            500 => StatusCode(StatusCodes.Status500InternalServerError, result.message),
            _ => StatusCode(StatusCodes.Status501NotImplemented, new { error = $"Unhandled statuscode: {result.statusCode}" })
        };
    }
    
    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (identityUserId == null)
            return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Unauthorized: Invalid or missing session token" });

        var result = await _authService.GetProfile(identityUserId);

        return result.statusCode switch
        {
            200 => StatusCode(StatusCodes.Status200OK, result.data),
            404 => StatusCode(StatusCodes.Status404NotFound, result.message),
            500 => StatusCode(StatusCodes.Status500InternalServerError, result.message),
            _ => StatusCode(StatusCodes.Status501NotImplemented, new { error = $"Unhandled statuscode: {result.statusCode}" })
        };
    }
}
