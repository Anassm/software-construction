using System;
using System.Collections.Generic;

namespace v2.Core.DTOs
{
    public class DiscountCreateRequest
    {
        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? MaxUsage { get; set; }
        public decimal Percentage { get; set; } = 0m;
        public decimal? FixedAmount { get; set; }
        public string? AllowedLocation { get; set; }
    }

    public class DiscountUpdateRequest
    {
        public bool? IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? MaxUsage { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? FixedAmount { get; set; }
        public string? AllowedLocation { get; set; }
    }

    public class DiscountLinkUsersRequest
    {
        public List<Guid> UserIds { get; set; } = new();
        public List<string> Groups { get; set; } = new();
    }

    public class DiscountApplyRequest
    {
        public string Code { get; set; } = string.Empty;
        public decimal OriginalAmount { get; set; }
        public string? Location { get; set; }
    }

    public class DiscountApplyResult
    {
        public string Code { get; set; } = string.Empty;
        public decimal OriginalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
    }
}
