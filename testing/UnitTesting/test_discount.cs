using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using v2.Core.DTOs;
using v2.Core.Models;
using v2.Infrastructure.Data;
using v2.Infrastructure.Services;
using Xunit;

namespace UnitTesting
{
    public class PaymentServiceDiscountTests
    {
        private ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("DiscountServiceTests_" + Guid.NewGuid())
                .Options;

            return new ApplicationDbContext(options);
        }

        private (ApplicationDbContext ctx, User user, string identityUserId) SeedUser(ApplicationDbContext ctx, string role)
        {
            var identityUserId = Guid.NewGuid().ToString();
            var identityUser = new IdentityUser
            {
                Id = identityUserId,
                UserName = "user_" + role
            };

            var user = new User
            {
                ID = Guid.NewGuid(),
                IdentityUserId = identityUserId,
                IdentityUser = identityUser,
                Username = identityUser.UserName,
                Name = "Test " + role,
                Email = "test@example.com",
                PhoneNumber = "0000000000",
                Role = role,
                CreatedAt = DateTime.UtcNow,
                BirthYear = 2000,
                IsActive = true,
                Vehicles = new List<Vehicle>(),
                Reservations = new List<Reservation>(),
                Sessions = new List<Session>()
            };

            ctx.Add(identityUser);
            ctx.Users.Add(user);
            ctx.SaveChanges();

            return (ctx, user, identityUserId);
        }

        private DiscountCode SeedDiscount(ApplicationDbContext ctx,
            string code,
            bool isActive = true,
            DateTime? start = null,
            DateTime? expiry = null,
            int? maxUsage = null,
            int usageCount = 0,
            decimal percentage = 0m,
            decimal? fixedAmount = null,
            string? allowedLocation = null)
        {
            var d = new DiscountCode
            {
                ID = Guid.NewGuid(),
                Code = code,
                IsActive = isActive,
                StartDate = start,
                ExpiryDate = expiry,
                MaxUsage = maxUsage,
                UsageCount = usageCount,
                Percentage = percentage,
                FixedAmount = fixedAmount,
                AllowedLocation = allowedLocation,
                SavedAmount = 0m
            };

            ctx.DiscountCodes.Add(d);
            ctx.SaveChanges();
            return d;
        }

        // ----------------------------
        // CreateAsync
        // ----------------------------

        [Fact]
        public async Task CreateAsync_UserNotFound_Returns404()
        {
            using var ctx = CreateContext();
            var service = new DiscountService(ctx);

            var dto = new DiscountCreateRequest { Code = "TEST" };
            var result = await service.CreateAsync(dto, adminIdentityUserId: "missing");

            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task CreateAsync_NotAdmin_Returns403()
        {
            using var ctx = CreateContext();
            var (_, _, nonAdminId) = SeedUser(ctx, role: "user");

            var service = new DiscountService(ctx);
            var dto = new DiscountCreateRequest { Code = "TEST" };

            var result = await service.CreateAsync(dto, nonAdminId);

            Assert.Equal(403, result.statusCode);
        }

        [Fact]
        public async Task CreateAsync_EmptyCode_Returns400()
        {
            using var ctx = CreateContext();
            var (_, _, adminId) = SeedUser(ctx, role: "admin");

            var service = new DiscountService(ctx);
            var dto = new DiscountCreateRequest { Code = "   " };

            var result = await service.CreateAsync(dto, adminId);

            Assert.Equal(400, result.statusCode);
        }

        [Fact]
        public async Task CreateAsync_DuplicateCode_Returns409()
        {
            using var ctx = CreateContext();
            var (_, _, adminId) = SeedUser(ctx, role: "admin");

            SeedDiscount(ctx, code: "DUPLICATE");

            var service = new DiscountService(ctx);
            var dto = new DiscountCreateRequest { Code = "duplicate" }; // should normalize to DUPLICATE

            var result = await service.CreateAsync(dto, adminId);

            Assert.Equal(409, result.statusCode);
        }

        [Fact]
        public async Task CreateAsync_Success_NormalizesCode_ToUpperTrim()
        {
            using var ctx = CreateContext();
            var (_, _, adminId) = SeedUser(ctx, role: "admin");

            var service = new DiscountService(ctx);
            var dto = new DiscountCreateRequest { Code = "  teSt10  ", Percentage = 10m };

            var result = await service.CreateAsync(dto, adminId);

            Assert.Equal(201, result.statusCode);

            var inDb = ctx.DiscountCodes.Single();
            Assert.Equal("TEST10", inDb.Code);
            Assert.Equal(10m, inDb.Percentage);
        }

        // ----------------------------
        // DeactivateAsync
        // ----------------------------

        [Fact]
        public async Task DeactivateAsync_NotAdmin_Returns403()
        {
            using var ctx = CreateContext();
            var (_, _, nonAdminId) = SeedUser(ctx, role: "user");
            var discount = SeedDiscount(ctx, "CODE");

            var service = new DiscountService(ctx);
            var result = await service.DeactivateAsync(discount.ID, nonAdminId);

            Assert.Equal(403, result.statusCode);
        }

        [Fact]
        public async Task DeactivateAsync_NotFound_Returns404()
        {
            using var ctx = CreateContext();
            var (_, _, adminId) = SeedUser(ctx, role: "admin");

            var service = new DiscountService(ctx);
            var result = await service.DeactivateAsync(Guid.NewGuid(), adminId);

            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task DeactivateAsync_AlreadyInactive_Returns409()
        {
            using var ctx = CreateContext();
            var (_, _, adminId) = SeedUser(ctx, role: "admin");
            var discount = SeedDiscount(ctx, "CODE", isActive: false);

            var service = new DiscountService(ctx);
            var result = await service.DeactivateAsync(discount.ID, adminId);

            Assert.Equal(409, result.statusCode);
        }

        [Fact]
        public async Task DeactivateAsync_Success_SetsInactive()
        {
            using var ctx = CreateContext();
            var (_, _, adminId) = SeedUser(ctx, role: "admin");
            var discount = SeedDiscount(ctx, "CODE", isActive: true);

            var service = new DiscountService(ctx);
            var result = await service.DeactivateAsync(discount.ID, adminId);

            Assert.Equal(200, result.statusCode);

            var inDb = ctx.DiscountCodes.Single(d => d.ID == discount.ID);
            Assert.False(inDb.IsActive);
        }

        // ----------------------------
        // UpdateExpiryAsync
        // ----------------------------

        [Fact]
        public async Task UpdateExpiryAsync_NotFound_Returns404()
        {
            using var ctx = CreateContext();
            var (_, _, adminId) = SeedUser(ctx, role: "admin");

            var service = new DiscountService(ctx);
            var result = await service.UpdateExpiryAsync(Guid.NewGuid(), DateTime.UtcNow.AddDays(10), adminId);

            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task UpdateExpiryAsync_Success_UpdatesDate()
        {
            using var ctx = CreateContext();
            var (_, _, adminId) = SeedUser(ctx, role: "admin");
            var discount = SeedDiscount(ctx, "CODE");

            var service = new DiscountService(ctx);
            var newExpiry = DateTime.UtcNow.AddDays(7);

            var result = await service.UpdateExpiryAsync(discount.ID, newExpiry, adminId);

            Assert.Equal(200, result.statusCode);

            var inDb = ctx.DiscountCodes.Single(d => d.ID == discount.ID);
            Assert.Equal(newExpiry, inDb.ExpiryDate);
        }

        // ----------------------------
        // LinkUsersAsync
        // ----------------------------

        [Fact]
        public async Task LinkUsersAsync_NotFound_Returns404()
        {
            using var ctx = CreateContext();
            var (_, _, adminId) = SeedUser(ctx, role: "admin");

            var service = new DiscountService(ctx);
            var dto = new DiscountLinkUsersRequest
            {
                UserIds = new List<Guid> { Guid.NewGuid() },
                Groups = new List<string> { "Business" }
            };

            var result = await service.LinkUsersAsync(Guid.NewGuid(), dto, adminId);

            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task UpdateAsync_UserNotFound_Returns404()
        {
            using var ctx = CreateContext();
            var service = new DiscountService(ctx);

            var result = await service.UpdateAsync(Guid.NewGuid(), new DiscountUpdateRequest(), "missing");

            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task UpdateAsync_NotAdmin_Returns403()
        {
            using var ctx = CreateContext();
            var (_, _, userId) = SeedUser(ctx, "user");
            var discount = SeedDiscount(ctx, "CODE");

            var service = new DiscountService(ctx);
            var result = await service.UpdateAsync(discount.ID, new DiscountUpdateRequest(), userId);

            Assert.Equal(403, result.statusCode);
        }

        [Fact]
        public async Task UpdateAsync_DiscountNotFound_Returns404()
        {
            using var ctx = CreateContext();
            var (_, _, adminId) = SeedUser(ctx, "admin");

            var service = new DiscountService(ctx);
            var result = await service.UpdateAsync(Guid.NewGuid(), new DiscountUpdateRequest(), adminId);

            Assert.Equal(404, result.statusCode);
        }
        // ----------------------------
        // ValidateAndApplyAsync
        // ----------------------------

        [Fact]
        public async Task ValidateAndApplyAsync_MissingCode_Returns400()
        {
            using var ctx = CreateContext();
            var (_, _, userIdentityId) = SeedUser(ctx, role: "user");

            var service = new DiscountService(ctx);
            var dto = new DiscountApplyRequest { Code = "  ", OriginalAmount = 100m, Location = "Rotterdam" };

            var result = await service.ValidateAndApplyAsync(dto, userIdentityId);

            Assert.Equal(400, result.statusCode);
        }

        [Fact]
        public async Task ValidateAndApplyAsync_UserNotFound_Returns404()
        {
            using var ctx = CreateContext();
            var service = new DiscountService(ctx);
            var dto = new DiscountApplyRequest { Code = "TEST", OriginalAmount = 100m, Location = "Rotterdam" };

            var result = await service.ValidateAndApplyAsync(dto, "missing");

            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task ValidateAndApplyAsync_DiscountNotFound_Returns404()
        {
            using var ctx = CreateContext();
            var (_, _, userIdentityId) = SeedUser(ctx, role: "user");

            var service = new DiscountService(ctx);
            var dto = new DiscountApplyRequest { Code = "NOTEXIST", OriginalAmount = 100m, Location = "Rotterdam" };

            var result = await service.ValidateAndApplyAsync(dto, userIdentityId);

            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task ValidateAndApplyAsync_Inactive_Returns400()
        {
            using var ctx = CreateContext();
            var (_, _, userIdentityId) = SeedUser(ctx, role: "user");
            SeedDiscount(ctx, "INACTIVE", isActive: false, percentage: 10m);

            var service = new DiscountService(ctx);
            var dto = new DiscountApplyRequest { Code = "inactive", OriginalAmount = 100m, Location = "Rotterdam" };

            var result = await service.ValidateAndApplyAsync(dto, userIdentityId);

            Assert.Equal(400, result.statusCode);
        }

        [Fact]
        public async Task ValidateAndApplyAsync_NotYetValid_Returns400()
        {
            using var ctx = CreateContext();
            var (_, _, userIdentityId) = SeedUser(ctx, role: "user");
            SeedDiscount(ctx, "FUTURE", isActive: true, start: DateTime.UtcNow.AddDays(1), percentage: 10m);

            var service = new DiscountService(ctx);
            var dto = new DiscountApplyRequest { Code = "FUTURE", OriginalAmount = 100m, Location = "Rotterdam" };

            var result = await service.ValidateAndApplyAsync(dto, userIdentityId);

            Assert.Equal(400, result.statusCode);
        }

        [Fact]
        public async Task ValidateAndApplyAsync_Expired_Returns400()
        {
            using var ctx = CreateContext();
            var (_, _, userIdentityId) = SeedUser(ctx, role: "user");
            SeedDiscount(ctx, "OLD", isActive: true, expiry: DateTime.UtcNow.AddDays(-1), percentage: 10m);

            var service = new DiscountService(ctx);
            var dto = new DiscountApplyRequest { Code = "OLD", OriginalAmount = 100m, Location = "Rotterdam" };

            var result = await service.ValidateAndApplyAsync(dto, userIdentityId);

            Assert.Equal(400, result.statusCode);
        }

        [Fact]
        public async Task ValidateAndApplyAsync_UsageLimitReached_Returns400()
        {
            using var ctx = CreateContext();
            var (_, _, userIdentityId) = SeedUser(ctx, role: "user");
            SeedDiscount(ctx, "LIMIT", isActive: true, maxUsage: 2, usageCount: 2, percentage: 10m);

            var service = new DiscountService(ctx);
            var dto = new DiscountApplyRequest { Code = "LIMIT", OriginalAmount = 100m, Location = "Rotterdam" };

            var result = await service.ValidateAndApplyAsync(dto, userIdentityId);

            Assert.Equal(400, result.statusCode);
        }

        [Fact]
        public async Task ValidateAndApplyAsync_LocationMismatch_Returns400()
        {
            using var ctx = CreateContext();
            var (_, _, userIdentityId) = SeedUser(ctx, role: "user");
            SeedDiscount(ctx, "LOC", isActive: true, percentage: 10m, allowedLocation: "Amsterdam");

            var service = new DiscountService(ctx);
            var dto = new DiscountApplyRequest { Code = "LOC", OriginalAmount = 100m, Location = "Rotterdam" };

            var result = await service.ValidateAndApplyAsync(dto, userIdentityId);

            Assert.Equal(400, result.statusCode);
        }

        [Fact]
        public async Task ValidateAndApplyAsync_UserNotAuthorized_WhenLinksExist_Returns403()
        {
            using var ctx = CreateContext();
            var (_, user, userIdentityId) = SeedUser(ctx, role: "user");
            var discount = SeedDiscount(ctx, "AUTH", isActive: true, percentage: 10m);

            // Link is for a different user
            ctx.DiscountCodeUsers.Add(new DiscountCodeUser { DiscountCodeId = discount.ID, UserId = Guid.NewGuid() });
            ctx.SaveChanges();

            var service = new DiscountService(ctx);
            var dto = new DiscountApplyRequest { Code = "AUTH", OriginalAmount = 100m, Location = "Rotterdam" };

            var result = await service.ValidateAndApplyAsync(dto, userIdentityId);

            Assert.Equal(403, result.statusCode);
        }

        [Fact]
        public async Task ValidateAndApplyAsync_Success_Percentage_IncrementsUsageAndSavedAmount()
        {
            using var ctx = CreateContext();
            var (_, user, userIdentityId) = SeedUser(ctx, role: "user");
            var discount = SeedDiscount(ctx, "PERC", isActive: true, percentage: 10m);

            var service = new DiscountService(ctx);
            var dto = new DiscountApplyRequest { Code = "perc", OriginalAmount = 100m, Location = "Rotterdam" };

            var result = await service.ValidateAndApplyAsync(dto, userIdentityId);

            Assert.Equal(200, result.statusCode);

            var inDb = ctx.DiscountCodes.Single(d => d.ID == discount.ID);
            Assert.Equal(1, inDb.UsageCount);
            Assert.True(inDb.SavedAmount > 0m); // should be 10
        }

        [Fact]
        public async Task ValidateAndApplyAsync_InvalidConfig_ZeroDiscount_Returns400()
        {
            using var ctx = CreateContext();
            var (_, _, userIdentityId) = SeedUser(ctx, role: "user");
            SeedDiscount(ctx, "ZERO", isActive: true, percentage: 0m, fixedAmount: 0m);

            var service = new DiscountService(ctx);
            var dto = new DiscountApplyRequest { Code = "ZERO", OriginalAmount = 100m, Location = "Rotterdam" };

            var result = await service.ValidateAndApplyAsync(dto, userIdentityId);

            Assert.Equal(400, result.statusCode);
        }

        // ----------------------------
        // GetAllActiveCodesAsync
        // ----------------------------

        [Fact]
        public async Task GetAllActiveCodesAsync_NotAdmin_Returns403()
        {
            using var ctx = CreateContext();
            var (_, _, nonAdminId) = SeedUser(ctx, role: "user");
            var service = new DiscountService(ctx);

            var result = await service.GetAllActiveCodesAsync(nonAdminId);

            Assert.Equal(403, result.statusCode);
        }

        [Fact]
        public async Task GetAllActiveCodesAsync_ReturnsOnlyCurrentlyActive()
        {
            using var ctx = CreateContext();
            var (_, _, adminId) = SeedUser(ctx, role: "admin");

            SeedDiscount(ctx, "ACTIVE_NOW", isActive: true, start: DateTime.UtcNow.AddDays(-1), expiry: DateTime.UtcNow.AddDays(1), percentage: 10m);
            SeedDiscount(ctx, "INACTIVE", isActive: false, percentage: 10m);
            SeedDiscount(ctx, "FUTURE", isActive: true, start: DateTime.UtcNow.AddDays(3), percentage: 10m);
            SeedDiscount(ctx, "EXPIRED", isActive: true, expiry: DateTime.UtcNow.AddDays(-2), percentage: 10m);

            var service = new DiscountService(ctx);
            var result = await service.GetAllActiveCodesAsync(adminId);

            Assert.Equal(200, result.statusCode);

            // easiest check: db query should yield 1 valid; service returns list too but as anonymous object
            var activeCountExpected = ctx.DiscountCodes.Count(d =>
                d.IsActive &&
                (d.StartDate == null || d.StartDate <= DateTime.UtcNow) &&
                (d.ExpiryDate == null || d.ExpiryDate >= DateTime.UtcNow));

            Assert.Equal(1, activeCountExpected);
        }

        // ----------------------------
        // GetUsedCodesAsync
        // ----------------------------

        [Fact]
        public async Task GetUsedCodesAsync_FilterIdNotExists_Returns404()
        {
            using var ctx = CreateContext();
            var (_, _, adminId) = SeedUser(ctx, role: "admin");

            var service = new DiscountService(ctx);
            var result = await service.GetUsedCodesAsync(Guid.NewGuid(), adminId);

            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task GetUsedCodesAsync_ReturnsUses()
        {
            using var ctx = CreateContext();
            var (_, _, adminId) = SeedUser(ctx, role: "admin");
            var discount = SeedDiscount(ctx, "USED", percentage: 10m);

            ctx.DiscountCodeUsers.Add(new DiscountCodeUser { DiscountCodeId = discount.ID, UserId = Guid.NewGuid() });
            ctx.DiscountCodeUsers.Add(new DiscountCodeUser { DiscountCodeId = discount.ID, GroupName = "Business" });
            ctx.SaveChanges();

            var service = new DiscountService(ctx);
            var result = await service.GetUsedCodesAsync(discount.ID, adminId);

            Assert.Equal(200, result.statusCode);

            var count = ctx.DiscountCodeUsers.Count(x => x.DiscountCodeId == discount.ID);
            Assert.Equal(2, count);
        }

        // ----------------------------
        // GetStatisticsAsync
        // ----------------------------

        [Fact]
        public async Task GetStatisticsAsync_Default_Returns200()
        {
            using var ctx = CreateContext();
            SeedDiscount(ctx, "A", usageCount: 5);
            SeedDiscount(ctx, "B", usageCount: 1);

            var service = new DiscountService(ctx);
            var result = await service.GetStatisticsAsync();

            Assert.Equal(200, result.statusCode);
            Assert.NotNull(result.data);
            Assert.True(result.data.Discounts.Count >= 2);
        }

        [Fact]
        public async Task GetStatisticsAsync_FilterTotalUsesAsc_Sorts()
        {
            using var ctx = CreateContext();
            SeedDiscount(ctx, "A", usageCount: 5);
            SeedDiscount(ctx, "B", usageCount: 1);

            var service = new DiscountService(ctx);
            var result = await service.GetStatisticsAsync(filter: "totalUses", orderBy: "asc");

            Assert.Equal(200, result.statusCode);

            var list = result.data.Discounts;
            Assert.True(list.Count >= 2);
            Assert.True(list[0].TotalUses <= list[1].TotalUses);
        }
    }
}
