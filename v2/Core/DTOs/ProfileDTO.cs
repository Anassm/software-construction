namespace v2.Core.DTOs;
public class ProfileDto
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; } 
    public string Name { get; set; }
    public string Phone { get; set; } 
    public string Role { get; set; }
    public DateTime Created_at { get; set; }
    public int? Birth_year { get; set; }
    public bool Active { get; set; }
}