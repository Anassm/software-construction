namespace v2.Core.DTOs;
public class LoginDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}


public class RegisterDto
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
}