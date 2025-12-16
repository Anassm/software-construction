using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using v2.Core.DTOs;
using v2.Core.Models;
using v2.Infrastructure.Data;
using v2.infrastructure.Services;
using Xunit;

namespace UnitTesting
{
    public class OrganizationServiceTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        private OrganizationService GetService(ApplicationDbContext context)
        {
            return new OrganizationService(context);
        }

        [Fact]
        public async Task CreateOrganization_Should_Succeed_When_Valid()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var service = GetService(context);

            var dto = new OrganizationCreateRequest
            {
                Name = "Test Org",
                Address = "Teststraat 1",
                ContactEmail = "info@test.org",
                ContactPhone = "0612345678"
            };

            var (statusCode, result) = await service.CreateAsync(dto);

            Assert.Equal(201, statusCode);
            Assert.Equal(1, context.Set<Organization>().Count());
            var org = context.Set<Organization>().First();
            Assert.Equal(dto.Name, org.Name);
            Assert.Equal(dto.Address, org.Address);
        }

        [Fact]
        public async Task CreateOrganization_Should_Fail_When_Duplicate()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);

            var existing = new Organization
            {
                Name = "Roffa Parking",
                Address = "Rotterdam",
                ContactEmail = "contact@roffa.nl",
                ContactPhone = "0612345678"
            };
            context.Organizations.Add(existing);
            await context.SaveChangesAsync();

            var service = GetService(context);

            var dto = new OrganizationCreateRequest
            {
                Name = "Roffa Parking",
                Address = "Rotterdam",
                ContactEmail = "nieuw@roffa.nl",
                ContactPhone = "0699999999"
            };

            var (statusCode, result) = await service.CreateAsync(dto);

            Assert.Equal(409, statusCode);
            Assert.Equal(1, context.Organizations.Count());
        }

        [Fact]
        public async Task UpdateOrganization_Should_Succeed_When_Valid()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);

            var org = new Organization
            {
                Name = "Old Name",
                Address = "Old Address"
            };
            context.Organizations.Add(org);
            await context.SaveChangesAsync();

            var service = GetService(context);

            var dto = new OrganizationUpdateRequest
            {
                Name = "New Name",
                Address = "New Address",
                ContactEmail = "new@mail.com",
                ContactPhone = "0611111111"
            };

            var (statusCode, result) = await service.UpdateAsync(org.ID, dto);

            Assert.Equal(200, statusCode);
            var updated = context.Organizations.First(o => o.ID == org.ID);
            Assert.Equal("New Name", updated.Name);
            Assert.Equal("New Address", updated.Address);
            Assert.Equal("new@mail.com", updated.ContactEmail);
            Assert.Equal("0611111111", updated.ContactPhone);
        }

        [Fact]
        public async Task UpdateOrganization_Should_Return404_When_NotFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var service = GetService(context);

            var dto = new OrganizationUpdateRequest
            {
                Name = "DoesNotMatter"
            };

            var (statusCode, result) = await service.UpdateAsync(Guid.NewGuid(), dto);

            Assert.Equal(404, statusCode);
        }

        [Fact]
        public async Task DeleteOrganization_Should_Return409_When_Has_Relations()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);

            var org = new Organization
            {
                Name = "Org with stuff",
                Address = "Somewhere"
            };

            var user = new User
            {
                ID = Guid.NewGuid(),
                OldID = "",
                IdentityUserId = "dummy",
                IdentityUser = new Microsoft.AspNetCore.Identity.IdentityUser { Id = "dummy", UserName = "dummy" },
                Username = "testuser",
                Name = "Test User",
                Email = "user@mail.com",
                PhoneNumber = "0600000000",
                Role = "User",
                IsActive = true,
                BirthYear = 2000,
                Organization = org,
                Vehicles = new System.Collections.Generic.List<Vehicle>(),
                Sessions = new System.Collections.Generic.List<Session>(),
                Reservations = new System.Collections.Generic.List<Reservation>(),
                Invoices = new System.Collections.Generic.List<Invoice>()
            };

            var discount = new DiscountCode
            {
                Code = "ORG10",
                IsActive = true,
                Percentage = 10,
                Organization = org
            };

            context.Organizations.Add(org);
            context.Users.Add(user);
            context.DiscountCodes.Add(discount);
            await context.SaveChangesAsync();

            var service = GetService(context);

            var (statusCode, result) = await service.DeleteAsync(org.ID);

            Assert.Equal(409, statusCode);
            Assert.NotNull(await context.Organizations.FindAsync(org.ID));
        }

        [Fact]
        public async Task DeleteOrganization_Should_Succeed_When_No_Relations()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);

            var org = new Organization
            {
                Name = "Delete Me",
                Address = "Here"
            };
            context.Organizations.Add(org);
            await context.SaveChangesAsync();

            var service = GetService(context);

            var (statusCode, result) = await service.DeleteAsync(org.ID);

            Assert.Equal(200, statusCode);
            Assert.Null(await context.Organizations.FindAsync(org.ID));
        }

        [Fact]
        public async Task GetAllOrganizations_Should_Return_List()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);

            context.Organizations.Add(new Organization { Name = "Org1", Address = "A" });
            context.Organizations.Add(new Organization { Name = "Org2", Address = "B" });
            await context.SaveChangesAsync();

            var service = GetService(context);

            var (statusCode, result) = await service.GetAllAsync();

            Assert.Equal(200, statusCode);

            var json = System.Text.Json.JsonSerializer.Serialize(result);

            var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            Assert.NotNull(dict);
            Assert.True(dict.ContainsKey("organizations"));

            var orgJson = dict["organizations"].ToString();
            var orgList = System.Text.Json.JsonSerializer.Deserialize<List<OrganizationSummaryDto>>(orgJson);

            Assert.NotNull(orgList);
            Assert.True(orgList.Count >= 2);
        }


        [Fact]
        public async Task GetById_Should_Return404_When_NotFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = GetDbContext(dbName);
            var service = GetService(context);

            var (statusCode, result) = await service.GetByIdAsync(Guid.NewGuid());

            Assert.Equal(404, statusCode);
        }
    }
}
