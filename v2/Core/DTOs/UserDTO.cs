namespace v2.Core.DTOs;
public class LoginDto
{
    public string username { get; set; }
    public string password { get; set; }
}


public class RegisterDto
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }

}