using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using v2.Core.DTOs;
using v2.Core.Interfaces;
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
                .UseInMemoryDatabase("PaymentDiscountTests_" + Guid.NewGuid())
                .Options;

            return new ApplicationDbContext(options);
        }

        private (ApplicationDbContext ctx, User user, string identityUserId) SeedUser(ApplicationDbContext ctx)
        {
            var identityUserId = Guid.NewGuid().ToString();
            var identityUser = new IdentityUser
            {
                Id = identityUserId,
                UserName = "testuser"
            };

            var user = new User
            {
                ID = Guid.NewGuid(),
                OldID = "",
                IdentityUserId = identityUserId,
                IdentityUser = identityUser,
                Username = "testuser",
                Name = "Test User",
                Email = "test@example.com",
                PhoneNumber = "0000000000",
                Role = "user",
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

        [Fact]
        public async Task CreatePayment_WithoutDiscount_UsesOriginalAmount()
        {
            var ctx = CreateContext();
            var (context, user, identityUserId) = SeedUser(ctx);

            var fakeDiscount = new FakeDiscountService
            {
                Mode = FakeDiscountMode.NeverCalled
            };

            var service = new PaymentService(context, fakeDiscount);

            var request = new CreatePaymentRequestDTO
            {
                Amount = 100m,
                SessionID = Guid.NewGuid(),
                Transaction = null,
                TransactionAmount = null,
                TransactionDate = null,
                TransactionMethod = "ideal",
                TransactionIssuer = "bank",
                TransactionBank = "ING",
                DiscountCode = null,
                Location = null
            };

            var result = await service.CreatePaymentAsync(request, identityUserId);

            Assert.Equal(201, result.statusCode);
            Assert.False(fakeDiscount.WasValidateCalled);

            var paymentsInDb = context.Payments.ToList();
            Assert.Single(paymentsInDb);
            Assert.Equal(100m, paymentsInDb[0].Amount);
        }

        [Fact]
        public async Task CreatePayment_WithValidDiscount_AppliesDiscount()
        {
            var ctx = CreateContext();
            var (context, user, identityUserId) = SeedUser(ctx);

            var fakeDiscount = new FakeDiscountService
            {
                Mode = FakeDiscountMode.Success,
                ConfiguredDiscountAmount = 10m
            };

            var service = new PaymentService(context, fakeDiscount);

            var request = new CreatePaymentRequestDTO
            {
                Amount = 100m,
                SessionID = Guid.NewGuid(),
                Transaction = null,
                TransactionAmount = null,
                TransactionDate = null,
                TransactionMethod = "ideal",
                TransactionIssuer = "bank",
                TransactionBank = "ING",
                DiscountCode = "TEST10",
                Location = "Rotterdam"
            };

            var result = await service.CreatePaymentAsync(request, identityUserId);

            Assert.Equal(201, result.statusCode);
            Assert.True(fakeDiscount.WasValidateCalled);
            Assert.NotNull(fakeDiscount.LastApplyRequest);
            Assert.Equal("TEST10", fakeDiscount.LastApplyRequest.Code);
            Assert.Equal(100m, fakeDiscount.LastApplyRequest.OriginalAmount);
            Assert.Equal("Rotterdam", fakeDiscount.LastApplyRequest.Location);

            var paymentsInDb = context.Payments.ToList();
            Assert.Single(paymentsInDb);
            Assert.Equal(90m, paymentsInDb[0].Amount);
        }

        [Fact]
        public async Task CreatePayment_WithDiscountError_ReturnsErrorAndNoPaymentCreated()
        {
            var ctx = CreateContext();
            var (context, user, identityUserId) = SeedUser(ctx);

            var fakeDiscount = new FakeDiscountService
            {
                Mode = FakeDiscountMode.Error,
                ErrorStatusCode = 400,
                ErrorMessage = "Discount code has expired"
            };

            var service = new PaymentService(context, fakeDiscount);

            var request = new CreatePaymentRequestDTO
            {
                Amount = 100m,
                SessionID = Guid.NewGuid(),
                Transaction = null,
                TransactionAmount = null,
                TransactionDate = null,
                TransactionMethod = "ideal",
                TransactionIssuer = "bank",
                TransactionBank = "ING",
                DiscountCode = "OLD",
                Location = "Rotterdam"
            };

            var result = await service.CreatePaymentAsync(request, identityUserId);

            Assert.Equal(400, result.statusCode);
            Assert.True(fakeDiscount.WasValidateCalled);
            Assert.Contains("expired", result.data.ToString(), StringComparison.OrdinalIgnoreCase);

            var paymentsInDb = context.Payments.ToList();
            Assert.Empty(paymentsInDb);
        }

        private enum FakeDiscountMode
        {
            NeverCalled,
            Success,
            Error
        }

        private class FakeDiscountService : IDiscounts
        {
            public FakeDiscountMode Mode { get; set; } = FakeDiscountMode.NeverCalled;
            public bool WasValidateCalled { get; private set; }
            public DiscountApplyRequest? LastApplyRequest { get; private set; }
            public decimal ConfiguredDiscountAmount { get; set; } = 0m;
            public int ErrorStatusCode { get; set; } = 400;
            public string ErrorMessage { get; set; } = "Discount error";

            public Task<(int statusCode, object data)> CreateAsync(DiscountCreateRequest dto, string adminIdentityUserId)
            {
                throw new NotImplementedException();
            }

            public Task<(int statusCode, object data)> UpdateAsync(Guid id, DiscountUpdateRequest dto, string adminIdentityUserId)
            {
                throw new NotImplementedException();
            }

            public Task<(int statusCode, object data)> DeactivateAsync(Guid id, string adminIdentityUserId)
            {
                throw new NotImplementedException();
            }

            public Task<(int statusCode, object data)> UpdateExpiryAsync(Guid id, DateTime? expiryDate, string adminIdentityUserId)
            {
                throw new NotImplementedException();
            }

            public Task<(int statusCode, object data)> LinkUsersAsync(Guid id, DiscountLinkUsersRequest dto, string adminIdentityUserId)
            {
                throw new NotImplementedException();
            }

            public Task<(int statusCode, object data)> ValidateAndApplyAsync(DiscountApplyRequest dto, string identityUserId)
            {
                WasValidateCalled = true;
                LastApplyRequest = dto;

                if (Mode == FakeDiscountMode.Error)
                {
                    return Task.FromResult<(int, object)>((ErrorStatusCode, new { error = ErrorMessage }));
                }

                if (Mode == FakeDiscountMode.Success)
                {
                    var result = new DiscountApplyResult
                    {
                        Code = dto.Code,
                        OriginalAmount = dto.OriginalAmount,
                        DiscountAmount = ConfiguredDiscountAmount,
                        FinalAmount = dto.OriginalAmount - ConfiguredDiscountAmount
                    };

                    var payload = new
                    {
                        status = "Success",
                        discount = result
                    };

                    return Task.FromResult<(int, object)>((200, payload));
                }

                throw new InvalidOperationException("ValidateAndApplyAsync should not be called in this mode.");
            }

            public Task<(int statusCode, object data)> GetAllActiveCodesAsync(string adminIdentityUserId)
            {
                throw new NotImplementedException();
            }

            public Task<(int statusCode, object data)> GetUsedCodesAsync(Guid? discountCodeId, string adminIdentityUserId)
            {
                throw new NotImplementedException();
            }
        }
    }
}
