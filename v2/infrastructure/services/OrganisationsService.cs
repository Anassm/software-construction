using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using v2.Core.DTOs;
using v2.Core.Interfaces;
using v2.Core.Models;
using v2.Infrastructure.Data;

namespace v2.infrastructure.Services
{
    public class OrganizationService : IOrganizations
    {
        private readonly ApplicationDbContext _db;

        public OrganizationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<(int statusCode, object data)> CreateAsync(OrganizationCreateRequest dto)
        {
            try
            {
                var duplicate = await _db.Organizations
                    .FirstOrDefaultAsync(o => o.Name == dto.Name && o.Address == dto.Address);

                if (duplicate != null)
                {
                    return (409, new
                    {
                        error = "Organization already exists",
                        data = new { duplicate.ID, duplicate.Name, duplicate.Address }
                    });
                }

                var org = new Organization
                {
                    Name = dto.Name,
                    Address = dto.Address,
                    ContactEmail = dto.ContactEmail,
                    ContactPhone = dto.ContactPhone,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Organizations.Add(org);
                await _db.SaveChangesAsync();

                return (201, new
                {
                    status = "Success",
                    organization = new
                    {
                        id = org.ID,
                        org.Name,
                        org.Address,
                        org.ContactEmail,
                        org.ContactPhone,
                        org.CreatedAt
                    }
                });
            }
            catch
            {
                return (500, new { error = "An unexpected error occurred." });
            }
        }

        public async Task<(int statusCode, object data)> UpdateAsync(Guid id, OrganizationUpdateRequest dto)
        {
            try
            {
                var org = await _db.Organizations.FirstOrDefaultAsync(o => o.ID == id);
                if (org == null)
                    return (404, new { error = "Organization not found" });

                var newName = dto.Name ?? org.Name;
                var newAddress = dto.Address ?? org.Address;

                var duplicate = await _db.Organizations
                    .FirstOrDefaultAsync(o =>
                        o.ID != id &&
                        o.Name == newName &&
                        o.Address == newAddress);

                if (duplicate != null)
                {
                    return (409, new
                    {
                        error = "Organization with same name and address already exists",
                        data = new { duplicate.ID, duplicate.Name, duplicate.Address }
                    });
                }

                if (!string.IsNullOrWhiteSpace(dto.Name)) org.Name = dto.Name;
                if (!string.IsNullOrWhiteSpace(dto.Address)) org.Address = dto.Address;
                if (!string.IsNullOrWhiteSpace(dto.ContactEmail)) org.ContactEmail = dto.ContactEmail;
                if (!string.IsNullOrWhiteSpace(dto.ContactPhone)) org.ContactPhone = dto.ContactPhone;
                org.UpdatedAt = DateTime.UtcNow;

                _db.Organizations.Update(org);
                await _db.SaveChangesAsync();

                return (200, new
                {
                    status = "Success",
                    organization = new
                    {
                        id = org.ID,
                        org.Name,
                        org.Address,
                        org.ContactEmail,
                        org.ContactPhone,
                        org.CreatedAt,
                        org.UpdatedAt
                    }
                });
            }
            catch
            {
                return (500, new { error = "An unexpected error occurred." });
            }
        }

        public async Task<(int statusCode, object data)> DeleteAsync(Guid id)
        {
            try
            {
                var org = await _db.Organizations
                    .Include(o => o.Users)
                    .ThenInclude(u => u.Vehicles)
                    .Include(o => o.DiscountCodes)
                    .FirstOrDefaultAsync(o => o.ID == id);

                if (org == null)
                    return (404, new { error = "Organization not found" });

                var hasUsers = org.Users.Any();
                var hasVehicles = org.Users.SelectMany(u => u.Vehicles).Any();
                var hasDiscounts = org.DiscountCodes.Any();

                if (hasUsers || hasVehicles || hasDiscounts)
                {
                    return (409, new
                    {
                        error = "Organization cannot be deleted because it has related users, vehicles or discount codes."
                    });
                }

                _db.Organizations.Remove(org);
                await _db.SaveChangesAsync();

                return (200, new { status = "Success", message = "Organization deleted" });
            }
            catch
            {
                return (500, new { error = "An unexpected error occurred." });
            }
        }

        public async Task<(int statusCode, object data)> GetAllAsync()
        {
            try
            {
                var organizations = await _db.Organizations
                    .Select(o => new OrganizationSummaryDto
                    {
                        Id = o.ID,
                        Name = o.Name,
                        Address = o.Address,
                        UserCount = o.Users.Count,
                        VehicleCount = o.Users.SelectMany(u => u.Vehicles).Count(),
                        DiscountCodeCount = o.DiscountCodes.Count
                    })
                    .ToListAsync();

                return (200, new
                {
                    status = "Success",
                    organizations
                });
            }
            catch
            {
                return (500, new { error = "An unexpected error occurred." });
            }
        }

        public async Task<(int statusCode, object data)> GetByIdAsync(Guid id)
        {
            try
            {
                var org = await _db.Organizations
                    .Include(o => o.Users)
                        .ThenInclude(u => u.Vehicles)
                    .Include(o => o.DiscountCodes)
                    .FirstOrDefaultAsync(o => o.ID == id);

                if (org == null)
                    return (404, new { error = "Organization not found" });

                var dto = new OrganizationDetailDto
                {
                    Id = org.ID,
                    Name = org.Name,
                    Address = org.Address,
                    ContactEmail = org.ContactEmail,
                    ContactPhone = org.ContactPhone,
                    CreatedAt = org.CreatedAt,
                    UpdatedAt = org.UpdatedAt,
                    Users = org.Users.Select(u => new OrganizationUserDto
                    {
                        Id = u.ID,
                        Username = u.Username,
                        Name = u.Name,
                        Email = u.Email,
                        Role = u.Role
                    }).ToList(),
                    Vehicles = org.Users
                        .SelectMany(u => u.Vehicles)
                        .Select(v => new OrganizationVehicleDto
                        {
                            Id = v.ID,
                            LicensePlate = v.LicensePlate,
                            Make = v.Make,
                            Model = v.Model,
                            Color = v.Color,
                            Year = v.Year,
                            UserId = v.UserID
                        }).ToList(),
                    DiscountCodes = org.DiscountCodes.Select(d => new OrganizationDiscountCodeDto
                    {
                        Id = d.ID,
                        Code = d.Code,
                        IsActive = d.IsActive,
                        StartDate = d.StartDate,
                        ExpiryDate = d.ExpiryDate,
                        UsageCount = d.UsageCount,
                        MaxUsage = d.MaxUsage,
                        Percentage = d.Percentage,
                        FixedAmount = d.FixedAmount
                    }).ToList()
                };

                return (200, new { status = "Success", organization = dto });
            }
            catch
            {
                return (500, new { error = "An unexpected error occurred." });
            }
        }

        // public async Task<(int StatusCode, object data)> GetParkingActions(Guid organizationId)
        // {
            
        // }
    }
}
