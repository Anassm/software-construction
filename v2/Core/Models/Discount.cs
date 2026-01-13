using System;
using System.Collections.Generic;

namespace v2.Core.Models
{
    public class DiscountCode
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime? StartDate { get; set; } = null;
        public DateTime? ExpiryDate { get; set; } = null;
        public int? MaxUsage { get; set; } = null;
        public int UsageCount { get; set; } = 0;
        public ICollection<DiscountCodeUser> UserLinks { get; set; } = new List<DiscountCodeUser>();
        public string? AllowedLocation { get; set; }
        public decimal Percentage { get; set; } = 0m;
        public decimal SavedAmount { get; set; } = 0m;
        public decimal? FixedAmount { get; set; }

        public Guid? OrganizationId { get; set; }
        public Organization? Organization { get; set; }
    }

    public class DiscountCodeUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DiscountCodeId { get; set; }
        public DiscountCode DiscountCode { get; set; } = null!;
        public Guid? UserId { get; set; }
        public User? User { get; set; }
        public string? GroupName { get; set; }
    }
}
