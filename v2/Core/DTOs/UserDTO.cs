namespace v2.Core.DTOs;
public class LoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}


public class RegisterDto
{
    public required string Email { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
    public string PhoneNumber { get; set; } = "0000000000";
    public string Role { get; set; } = "user";
}