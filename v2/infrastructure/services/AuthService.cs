using Microsoft.EntityFrameworkCore;
using v2.core.Interfaces;
using v2.Core.Models;
using v2.Infrastructure.Data;
using v2.Core.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace v2.infrastructure.Services;

public class AuthService
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _dbContext;

    public AuthService(ApplicationDbContext dbContext, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<(User data, int statusCode, object message)> RegisterUser(RegisterDto dto)
    {
        try
        {
            var identityUser = new IdentityUser { UserName = dto.Username };
            var result = await _userManager.CreateAsync(identityUser, dto.Password);
            if (!result.Succeeded)
            {
                if (result.Errors.Any(e =>
                    e.Code == "PasswordTooShort" ||
                    e.Code == "PasswordRequiresNonAlphanumeric" ||
                    e.Code == "PasswordRequiresDigit" ||
                    e.Code == "PasswordRequiresUpper" ||
                    e.Code == "PasswordRequiresLower"))
                {
                    return (null!, 400, new { error = "An unexpected error occurred.", details = result.Errors });
                }
                return (null!, 400, new { error = "An unexpected error occurred.", details = result.Errors });
            }
            identityUser = await _userManager.FindByNameAsync(dto.Username);
            var appUser = new User
            {
                IdentityUserId = identityUser.Id,
                IdentityUser = identityUser,
                Username = dto.Username,
                Email = null,
                Name = dto.Name,
                PhoneNumber = null,
                BirthYear = null,
                Role = dto.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Vehicles = new List<Vehicle>(),
                Sessions = new List<Session>(),
                Reservations = new List<Reservation>()
            };

            _dbContext.Users.Add(appUser);
            await _dbContext.SaveChangesAsync();
            return (appUser, 201, new { message = "User registered successfully." });
        }
        catch
        {
            return (null!, 502, new { error = "An unexpected error occurred." });
        }
    }
    
    public async Task<(User data, int statusCode, object message)> LoginUser(LoginDto dto)
    {
        try
        {
            if(dto.username == "" || dto.password == "")
            {
                return (null!, 400, new { error = "Username and password are required." });
            }
            var user = await _userManager.FindByNameAsync(dto.username);
            if (user == null)
                return (null!, 404, new { error = "User not found" });

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.password, false);
            if (!result.Succeeded)
                return (null!, 401, new { error = "Invalid credentials" });

            // Generate JWT
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("thisIsASuperSecretKeyWithAtLeast32Bytes!"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: "yourIssuer",
                audience: "yourAudience",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return (null!, 200, new
            {
                tokentype = "Bearer",
                accesstoken = tokenString
            });
        }
        catch
        {
            return (null!, 500, new { error = "An unexpected error occurred." });
        }
    }

    public async Task<(ProfileDto data, int statusCode, object message)> GetProfile(string identityUserId)
    {
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
            if (user == null)
                return (null!, 404, new { error = "User not found" });
            var idUser = await _userManager.FindByIdAsync(user.IdentityUserId);
            var profileDto = new ProfileDto
            {
                Id = user.ID,
                Username = user.Username,
                Password = idUser.PasswordHash,
                Email = user.Email ?? "",
                Name = user.Name ?? "",
                Phone = user.PhoneNumber ?? "",
                Role = user.Role ?? "",
                Created_at = user.CreatedAt,
                Birth_year = user.BirthYear,
                Active = user.IsActive
            };

            return (profileDto, 200, new { });
        }
        catch(Exception ex)
        {
            return (null!, 500, new { error = "An unexpected error occurred.", details = ex.Message });
        }
    }

    

    public async Task<(User data, int statusCode, object message)> UpdateProfile( ProfileDto dto, string identityUserId)
    {
        try
        {

            var user = _dbContext.Users.FirstOrDefault(u => u.ID == dto.Id);
            var userASP = null as IdentityUser;
            try
            {
                userASP = await _userManager.FindByIdAsync(user.IdentityUserId);
            }
            catch
            {
                return (null!, 404, new { error = "User not found" });
            }



            var verify = _userManager.PasswordHasher.VerifyHashedPassword(userASP, userASP.PasswordHash, dto.Password);
            // If password has changed, update it
            if (verify != PasswordVerificationResult.Success)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(userASP);
                var result = await _userManager.ResetPasswordAsync(userASP, token, dto.Password);

            }

            // Update Email
            if (!string.Equals(userASP.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                var setEmailResult = await _userManager.SetEmailAsync(userASP, dto.Email);
                if (!setEmailResult.Succeeded)
                {
                    var errors = string.Join(", ", setEmailResult.Errors.Select(e => e.Description));
                    return (null!, 500, new { error = "An unexpected error occurred.", details = errors });
                }
            }

            // Update Username
            if (!string.Equals(userASP.UserName, dto.Username, StringComparison.OrdinalIgnoreCase))
            {
                var setUserNameResult = await _userManager.SetUserNameAsync(userASP, dto.Username);
                if (!setUserNameResult.Succeeded)
                {
                    var errors = string.Join(", ", setUserNameResult.Errors.Select(e => e.Description));
                    return (null!, 500, new { error = "An unexpected error occurred.", details = errors });
                }
            }

            if (user != null)
            {
                user.Username = dto.Username;
                user.Name = dto.Name;
                user.Email = dto.Email;
                user.PhoneNumber = dto.Phone;
                user.Role = dto.Role;
                user.CreatedAt = dto.Created_at;
                user.BirthYear = dto.Birth_year;
                user.IsActive = dto.Active;

                _dbContext.SaveChanges();
            }
            return (null!, 200, new { message = "Profile updated successfully." });
        }
        catch(Exception ex)
        {
            return (null!, 500, new { error = "An unexpected error occurred.", details = ex });
        }
    }
}