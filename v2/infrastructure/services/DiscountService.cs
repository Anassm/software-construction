using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using v2.Core.DTOs;
using v2.Core.Interfaces;
using v2.Core.Models;
using v2.Infrastructure.Data;

namespace v2.Infrastructure.Services
{
    public class DiscountService : IDiscounts
    {
        private readonly ApplicationDbContext _db;

        public DiscountService(ApplicationDbContext db)
        {
            _db = db;
        }

        private async Task<User?> GetUserByIdentityAsync(string identityUserId)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
        }

        private static bool IsAdmin(User user)
        {
            return string.Equals(user.Role, "admin", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<(int statusCode, object data)> CreateAsync(DiscountCreateRequest dto, string adminIdentityUserId)
        {
            var user = await GetUserByIdentityAsync(adminIdentityUserId);
            if (user == null)
                return (404, new { error = "User not found" });
            if (!IsAdmin(user))
                return (403, new { error = "Access denied. Admin role required." });

            if (string.IsNullOrWhiteSpace(dto.Code))
                return (400, new { error = "Required field missing, field: Code" });

            var normalizedCode = dto.Code.Trim().ToUpperInvariant();

            var existing = await _db.DiscountCodes
                .FirstOrDefaultAsync(d => d.Code == normalizedCode);

            if (existing != null)
                return (409, new { error = "Discount code already exists" });

            var discount = new DiscountCode
            {
                Code = normalizedCode,
                IsActive = dto.IsActive,
                StartDate = dto.StartDate,
                ExpiryDate = dto.ExpiryDate,
                MaxUsage = dto.MaxUsage,
                Percentage = dto.Percentage,
                FixedAmount = dto.FixedAmount,
                AllowedLocation = dto.AllowedLocation
            };

            _db.DiscountCodes.Add(discount);
            await _db.SaveChangesAsync();

            return (201, new
            {
                status = "Success",
                discount = new
                {
                    discount.ID,
                    discount.Code,
                    discount.IsActive,
                    discount.StartDate,
                    discount.ExpiryDate,
                    discount.MaxUsage,
                    discount.UsageCount,
                    discount.Percentage,
                    discount.FixedAmount,
                    discount.AllowedLocation
                }
            });
        }

        public async Task<(int statusCode, object data)> UpdateAsync(Guid id, DiscountUpdateRequest dto, string adminIdentityUserId)
        {
            var user = await GetUserByIdentityAsync(adminIdentityUserId);
            if (user == null)
                return (404, new { error = "User not found" });
            if (!IsAdmin(user))
                return (403, new { error = "Access denied. Admin role required." });

            var discount = await _db.DiscountCodes.FindAsync(id);
            if (discount == null)
                return (404, new { error = "Discount code not found" });

            if (dto.IsActive.HasValue) discount.IsActive = dto.IsActive.Value;
            if (dto.StartDate.HasValue) discount.StartDate = dto.StartDate;
            if (dto.ExpiryDate.HasValue) discount.ExpiryDate = dto.ExpiryDate;
            if (dto.MaxUsage.HasValue) discount.MaxUsage = dto.MaxUsage;
            if (dto.Percentage.HasValue) discount.Percentage = dto.Percentage.Value;
            if (dto.FixedAmount.HasValue) discount.FixedAmount = dto.FixedAmount;
            if (dto.AllowedLocation != null) discount.AllowedLocation = dto.AllowedLocation;

            _db.DiscountCodes.Update(discount);
            await _db.SaveChangesAsync();

            return (200, new
            {
                status = "Success",
                discount = new
                {
                    discount.ID,
                    discount.Code,
                    discount.IsActive,
                    discount.StartDate,
                    discount.ExpiryDate,
                    discount.MaxUsage,
                    discount.UsageCount,
                    discount.Percentage,
                    discount.FixedAmount,
                    discount.AllowedLocation
                }
            });
        }

        public async Task<(int statusCode, object data)> DeactivateAsync(Guid id, string adminIdentityUserId)
        {
            var user = await GetUserByIdentityAsync(adminIdentityUserId);
            if (user == null)
                return (404, new { error = "User not found" });
            if (!IsAdmin(user))
                return (403, new { error = "Access denied. Admin role required." });

            var discount = await _db.DiscountCodes.FindAsync(id);
            if (discount == null)
                return (404, new { error = "Discount code not found" });

            if (!discount.IsActive)
                return (409, new { error = "Discount code is already inactive" });

            discount.IsActive = false;
            _db.DiscountCodes.Update(discount);
            await _db.SaveChangesAsync();

            return (200, new { status = "Success", message = "Discount code deactivated" });
        }

        public async Task<(int statusCode, object data)> UpdateExpiryAsync(Guid id, DateTime? expiryDate, string adminIdentityUserId)
        {
            var user = await GetUserByIdentityAsync(adminIdentityUserId);
            if (user == null)
                return (404, new { error = "User not found" });
            if (!IsAdmin(user))
                return (403, new { error = "Access denied. Admin role required." });

            var discount = await _db.DiscountCodes.FindAsync(id);
            if (discount == null)
                return (404, new { error = "Discount code not found" });

            discount.ExpiryDate = expiryDate;
            _db.DiscountCodes.Update(discount);
            await _db.SaveChangesAsync();

            return (200, new { status = "Success", message = "Expiry date updated", expiryDate = discount.ExpiryDate });
        }

        public async Task<(int statusCode, object data)> LinkUsersAsync(Guid id, DiscountLinkUsersRequest dto, string adminIdentityUserId)
        {
            var user = await GetUserByIdentityAsync(adminIdentityUserId);
            if (user == null)
                return (404, new { error = "User not found" });
            if (!IsAdmin(user))
                return (403, new { error = "Access denied. Admin role required." });

            var discount = await _db.DiscountCodes
                .Include(d => d.UserLinks)
                .FirstOrDefaultAsync(d => d.ID == id);

            if (discount == null)
                return (404, new { error = "Discount code not found" });

            _db.DiscountCodeUsers.RemoveRange(discount.UserLinks);

            foreach (var uid in dto.UserIds.Distinct())
            {
                discount.UserLinks.Add(new DiscountCodeUser
                {
                    DiscountCodeId = discount.ID,
                    UserId = uid
                });
            }

            foreach (var group in dto.Groups.Where(g => !string.IsNullOrWhiteSpace(g)).Select(g => g.Trim()).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                discount.UserLinks.Add(new DiscountCodeUser
                {
                    DiscountCodeId = discount.ID,
                    GroupName = group
                });
            }

            await _db.SaveChangesAsync();

            return (200, new { status = "Success", message = "Links updated" });
        }

        public async Task<(int statusCode, object data)> ValidateAndApplyAsync(DiscountApplyRequest dto, string identityUserId)
        {
            if (string.IsNullOrWhiteSpace(dto.Code))
                return (400, new { error = "Required field missing, field: code" });

            var user = await GetUserByIdentityAsync(identityUserId);
            if (user == null)
                return (404, new { error = "User not found" });

            var now = DateTime.UtcNow;
            var normalizedCode = dto.Code.Trim().ToUpperInvariant();

            var discount = await _db.DiscountCodes
                .Include(d => d.UserLinks)
                .FirstOrDefaultAsync(d => d.Code == normalizedCode);

            if (discount == null)
                return (404, new { error = "Discount code not found" });

            if (!discount.IsActive)
                return (400, new { error = "Discount code is inactive" });

            if (discount.StartDate.HasValue && now < discount.StartDate.Value)
                return (400, new { error = "Discount code is not yet valid" });

            if (discount.ExpiryDate.HasValue && now > discount.ExpiryDate.Value)
                return (400, new { error = "Discount code has expired" });

            if (discount.MaxUsage.HasValue && discount.UsageCount >= discount.MaxUsage.Value)
                return (400, new { error = "Discount code usage limit reached" });

            if (!string.IsNullOrWhiteSpace(discount.AllowedLocation) &&
                !string.Equals(discount.AllowedLocation.Trim(), dto.Location?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return (400, new { error = "Discount code is not valid for this location" });
            }

            if (discount.UserLinks.Any())
            {
                var authorized = discount.UserLinks.Any(l =>
                    (l.UserId.HasValue && l.UserId == user.ID) ||
                    (!string.IsNullOrWhiteSpace(l.GroupName) &&
                     !string.IsNullOrWhiteSpace(user.Role) &&
                     string.Equals(l.GroupName, user.Role, StringComparison.OrdinalIgnoreCase)));

                if (!authorized)
                    return (403, new { error = "User is not authorized for this discount code" });
            }

            var original = dto.OriginalAmount;
            decimal discountAmount = 0m;

            if (discount.Percentage > 0)
            {
                discountAmount += Math.Round(original * (discount.Percentage / 100m), 2);
            }

            if (discount.FixedAmount.HasValue && discount.FixedAmount.Value > 0)
            {
                discountAmount += discount.FixedAmount.Value;
            }

            if (discountAmount <= 0)
                return (400, new { error = "Discount configuration invalid or results in zero discount" });

            var finalAmount = Math.Max(0, original - discountAmount);

            discount.UsageCount += 1;
            _db.DiscountCodes.Update(discount);
            await _db.SaveChangesAsync();

            var result = new DiscountApplyResult
            {
                Code = discount.Code,
                OriginalAmount = original,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount
            };

            return (200, new
            {
                status = "Success",
                discount = result
            });
        }
    }
}
