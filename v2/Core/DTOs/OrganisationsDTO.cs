using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace v2.Core.DTOs
{
    public class AssignUserDto
    {
        public required Guid OrganizationId { get; set; }
    }
    public class OrganizationCreateRequest
    {
        [Required]
        public string Name { get; set; } = null!;

        public string? Address { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
    }

    public class OrganizationUpdateRequest
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
    }

    public class OrganizationSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
        public int UserCount { get; set; }
        public int VehicleCount { get; set; }
        public int DiscountCodeCount { get; set; }
    }

    public class OrganizationUserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Email { get; set; }
        public string? Role { get; set; }
    }

    public class OrganizationVehicleDto
    {
        public Guid Id { get; set; }
        public string LicensePlate { get; set; } = null!;
        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? Color { get; set; }
        public int? Year { get; set; }
        public Guid UserId { get; set; }
    }

    public class OrganizationDiscountCodeDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int UsageCount { get; set; }
        public int? MaxUsage { get; set; }
        public decimal Percentage { get; set; }
        public decimal? FixedAmount { get; set; }
    }

    public class OrganizationDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public IEnumerable<OrganizationUserDto> Users { get; set; } = new List<OrganizationUserDto>();
        public IEnumerable<OrganizationVehicleDto> Vehicles { get; set; } = new List<OrganizationVehicleDto>();
        public IEnumerable<OrganizationDiscountCodeDto> DiscountCodes { get; set; } = new List<OrganizationDiscountCodeDto>();
    }

    public class OrganizationActionsResponse
    {
        public string Status { get; set; } = "Success";
        public List<OrganizationReservationDto> Reservations { get; set; } = [];
        public List<OrganizationSessionDto> Sessions { get; set; } = [];
    }

    public class OrganizationReservationDto
    {
        public Guid ID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid ParkingLotID { get; set; }
        public float TotalPrice { get; set; }
    }

    public class OrganizationSessionDto
    {
        public Guid ID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public Guid ParkingLotID { get; set; }
        public float Price { get; set; }
    }
}
