namespace v2.Core.Models;

using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
public class User
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public string OldID { get; set; } = "";
    public required string IdentityUserId { get; set; }
    public required IdentityUser IdentityUser { get; set; } 

    public required string Username { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public required DateTime BirthDate { get; set; }
    public required bool IsActive { get; set; } = true;

    public required ICollection<Vehicle> Vehicles { get; set; }
    public required ICollection<Session> Sessions { get; set; }
    public required ICollection<Reservation> Reservations { get; set; }
}