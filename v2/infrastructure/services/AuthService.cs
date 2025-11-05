using Microsoft.EntityFrameworkCore;
using v2.core.Interfaces;
using v2.Core.Models;
using v2.Infrastructure.Data;
using v2.Core.DTOs;

namespace v2.infrastructure.Services;

public class AuthService 
{
    private readonly ApplicationDbContext _dbContext;

    public AuthService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(User data, int statusCode, object message)> GetProfile(string identityUserId)
    {
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
            if (user == null)
                return (null!, 404, new { error = "User not found" });

            return (user, 200, new { });
        }
        catch
        {
            return (null!, 500, new { error = "An unexpected error occurred." });
        }
    }
}