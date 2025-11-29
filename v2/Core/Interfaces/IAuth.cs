using v2.Core.DTOs;
using v2.Core.Models;
using System.Threading.Tasks;

namespace v2.Core.Interfaces; 

public interface IAuth
{
    
    Task<(User? data, int statusCode, object message)> RegisterUser(RegisterDto dto);
    
    Task<(User? data, int statusCode, object message)> LoginUser(LoginDto dto);
    
    Task<(ProfileDto? data, int statusCode, object message)> GetProfile(string identityUserId);

    Task<(User? data, int statusCode, object message)> UpdateProfile(ProfileDto dto, string identityUserId);
    
    Task<(User? data, int statusCode, object message)> LogoutUser(string jti);

    Task<User?> GetUserByIdentityIdAsync(string identityUserId);
}