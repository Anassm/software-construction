namespace v2.Core.Models;

public class User
{
    public required int ID { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Role { get; set; }
    public required DateOnly CreatedAt { get; set; }
    public required DateOnly BirthDate { get; set; }
    public required bool IsActive { get; set; }
}