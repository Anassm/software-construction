namespace v2.Core.Models;

using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
public class User
{
    public Guid ID { get; set; } // Primary key for your domain table
    public required string IdentityUserId { get; set; } // FK to IdentityUser
    public required IdentityUser IdentityUser { get; set; } // navigation property

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