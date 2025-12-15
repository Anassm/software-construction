using System;
using System.Collections.Generic;

namespace v2.Core.Models
{
    public class Organization
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<DiscountCode> DiscountCodes { get; set; } = new List<DiscountCode>();

        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
