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
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _dbContext;

    public AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ApplicationDbContext dbContext)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var identityUser = new IdentityUser { UserName = dto.Username };
        var result = await _userManager.CreateAsync(identityUser, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var appUser = new User
        {
            IdentityUserId = identityUser.Id,
            IdentityUser = identityUser, 
            Username = dto.Username,
            Email = null,
            Name = dto.Name,
            PhoneNumber = null, 
            BirthDate = DateTime.UtcNow, 
            Role = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Vehicles = new List<Vehicle>(),
            Sessions = new List<Session>(),
            Reservations = new List<Reservation>()
        };

        _dbContext.Users.Add(appUser);
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "User registered successfully" });
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Username);
        if (user == null)
            return Unauthorized(new { error = "Invalid credentials" });

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
            return Unauthorized(new { error = "Invalid credentials" });

        // Generate JWT
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("yourSuperSecretKey"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "yourIssuer",
            audience: "yourAudience",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return Ok(new
        {
            tokenType = "Bearer",
            accessToken = new JwtSecurityTokenHandler().WriteToken(token)
        });
    }
    
//     [HttpPost("profile")]
//     public async Task<IActionResult> Profile([FromBody] ProfileDto dto)
//     {

//         var user = _dbContext.Users.FirstOrDefault(u => u.ID == dto.Id);

//         if (user != null)
//         {
//             user.Username = dto.Username;
//             user.Email = dto.Email;
//             user.PhoneNumber = dto.Phone;
//             user.Role = dto.Role;
//             user.IsActive = dto.Active;

//             _dbContext.SaveChanges();
//         }

//         return Ok(new
//         {
//             tokenType = "Bearer",
//             accessToken = new JwtSecurityTokenHandler().WriteToken(token)
//         });
//     }
}
