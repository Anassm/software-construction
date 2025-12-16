using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using v2.Infrastructure.Data;
using v2.Core.Models;
using v2.infrastructure.Services;
using v2.Core.DTOs;
using v2.Controllers;
using v2.core.Interfaces;

namespace UnitTesting
{
    public class ReservationServiceTests
    {
        private ApplicationDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private ParkingLot CreateTestParkingLot(Guid id)
        {
            return new ParkingLot
            {
                ID = id,
                OldID = "",
                Name = "Test Lot",
                Location = "Test City",
                Address = "Test Address",
                Capacity = 100,
                Reserved = 0,
                Tariff = 2.5f,
                DayTariff = 10f,
                CreatedAt = DateTime.UtcNow,
                latitude = 52.0f,
                longitude = 4.0f,
                Reservations = new List<Reservation>(),
                Sessions = new List<Session>()
            };
        }

        private Vehicle CreateTestVehicle(Guid id, Guid userId, string licensePlate)
        {
            return new Vehicle
            {
                ID = id,
                OldID = "",
                LicensePlate = licensePlate,
                Make = "TestMake",
                Model = "TestModel",
                Color = "Black",
                Year = 2022,
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                UserID = userId,
                User = null,
                Reservations = new List<Reservation>()
            };
        }

        private User CreateTestUser(Guid userId, string identityUserId = "identity-1")
        {
            var identity = new Microsoft.AspNetCore.Identity.IdentityUser
            {
                Id = identityUserId,
                UserName = "test.user"
            };

            return new User
            {
                ID = userId,
                OldID = "",
                IdentityUserId = identityUserId,
                IdentityUser = identity,
                Username = "test.user",
                Name = "Test User",
                Email = "test@example.com",
                PhoneNumber = "000",
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                BirthYear = 2000,
                IsActive = true,
                Vehicles = new List<Vehicle>(),
                Sessions = new List<Session>(),
                Reservations = new List<Reservation>()
            };
        }

        [Fact]
        public async Task CreateReservation_Should_Fail_When_EndDate_Not_After_StartDate()
        {
            using var db = CreateInMemoryDbContext();
            var lotId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var vehicleId = Guid.NewGuid();

            db.ParkingLots.Add(CreateTestParkingLot(lotId));
            db.Vehicles.Add(CreateTestVehicle(vehicleId, userId, "TEST123"));
            await db.SaveChangesAsync();

            var service = new ReservationService(db);
            var now = DateTime.UtcNow;

            var req = new ReservationCreateRequest
            {
                LicensePlate = "TEST-123",
                ParkingLotId = lotId,
                StartDate = now,
                EndDate = now
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateReservationAsync(req, "identity-1"));
            Assert.Contains("EndDate must be greater", ex.Message);
        }

        [Fact]
        public async Task CreateReservation_Should_Fail_When_ParkingLot_NotFound()
        {
            using var db = CreateInMemoryDbContext();
            var userId = Guid.NewGuid();
            var vehicleId = Guid.NewGuid();

            db.Vehicles.Add(CreateTestVehicle(vehicleId, userId, "TEST123"));
            await db.SaveChangesAsync();

            var service = new ReservationService(db);

            var req = new ReservationCreateRequest
            {
                LicensePlate = "TEST-123",
                StartDate = DateTime.UtcNow.AddHours(1),
                EndDate = DateTime.UtcNow.AddHours(2),
                ParkingLotId = Guid.NewGuid()
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateReservationAsync(req, "identity-1"));
            Assert.Contains("Parking lot not found", ex.Message);
        }

        [Fact]
        public async Task CreateReservation_Should_Fail_When_Vehicle_NotFound()
        {
            using var db = CreateInMemoryDbContext();
            var lotId = Guid.NewGuid();

            db.ParkingLots.Add(CreateTestParkingLot(lotId));
            await db.SaveChangesAsync();

            var service = new ReservationService(db);

            var req = new ReservationCreateRequest
            {
                LicensePlate = "DOES-NOT-EXIST",
                StartDate = DateTime.UtcNow.AddHours(1),
                EndDate = DateTime.UtcNow.AddHours(2),
                ParkingLotId = lotId
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateReservationAsync(req, "identity-1"));
            Assert.Contains("Vehicle with given license plate not found", ex.Message);
        }

        [Fact]
        public async Task CreateReservation_Should_Fail_When_LicensePlate_Empty_And_NoVehicleId()
        {
            using var db = CreateInMemoryDbContext();
            var lotId = Guid.NewGuid();
            db.ParkingLots.Add(CreateTestParkingLot(lotId));
            await db.SaveChangesAsync();

            var service = new ReservationService(db);

            var req = new ReservationCreateRequest
            {
                LicensePlate = "   ",
                StartDate = DateTime.UtcNow.AddHours(1),
                EndDate = DateTime.UtcNow.AddHours(2),
                ParkingLotId = lotId
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateReservationAsync(req, "identity-1"));
            Assert.Contains("License plate is required", ex.Message);
        }

        [Fact]
        public async Task CreateReservation_Should_Create_When_Valid_And_Normalize_LicensePlate()
        {
            using var db = CreateInMemoryDbContext();
            var lotId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var vehicleId = Guid.NewGuid();

            db.ParkingLots.Add(CreateTestParkingLot(lotId));
            db.Vehicles.Add(CreateTestVehicle(vehicleId, userId, "ABC123"));
            await db.SaveChangesAsync();

            var service = new ReservationService(db);
            var start = DateTime.UtcNow.AddHours(1);
            var end = DateTime.UtcNow.AddHours(3);

            var req = new ReservationCreateRequest
            {
                LicensePlate = "ABC-123",
                StartDate = start,
                EndDate = end,
                ParkingLotId = lotId,
                DiscountCode = "DISC10"
            };

            var created = await service.CreateReservationAsync(req, "identity-1");
            var saved = await db.Reservations.SingleAsync(r => r.ID == created.ID);

            Assert.Equal("Pending", saved.Status);
            Assert.Equal(vehicleId, saved.VehicleID);
            Assert.Equal(lotId, saved.ParkingLotID);
            Assert.Equal(userId, saved.UserID);
            Assert.Equal(start, saved.StartDate);
            Assert.Equal(end, saved.EndDate);
            Assert.Equal("DISC10", saved.DiscountCode);

            Assert.Equal("ABC123", req.LicensePlate);
        }

        [Fact]
        public async Task GetReservationsForUser_Should_Fail_When_User_NotFound()
        {
            using var db = CreateInMemoryDbContext();
            var service = new ReservationService(db);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetReservationsForUserAsync("missing-identity"));

            Assert.Contains("User not found", ex.Message);
        }

        [Fact]
        public async Task GetReservationsForUser_Should_Return_Only_Users_Reservations()
        {
            using var db = CreateInMemoryDbContext();

            var u1Id = Guid.NewGuid();
            var u2Id = Guid.NewGuid();

            var u1 = CreateTestUser(u1Id, "identity-u1");
            var u2 = CreateTestUser(u2Id, "identity-u2");
            db.Users.AddRange(u1, u2);

            var lotId = Guid.NewGuid();
            db.ParkingLots.Add(CreateTestParkingLot(lotId));

            var v1 = CreateTestVehicle(Guid.NewGuid(), u1Id, "U1CAR");
            var v2 = CreateTestVehicle(Guid.NewGuid(), u2Id, "U2CAR");
            db.Vehicles.AddRange(v1, v2);

            db.Reservations.AddRange(
                new Reservation
                {
                    ID = Guid.NewGuid(),
                    StartDate = DateTime.UtcNow.AddDays(2),
                    EndDate = DateTime.UtcNow.AddDays(3),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    TotalPrice = 0,
                    UserID = u1Id,
                    ParkingLotID = lotId,
                    VehicleID = v1.ID
                },
                new Reservation
                {
                    ID = Guid.NewGuid(),
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow.AddDays(1).AddHours(1),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    TotalPrice = 0,
                    UserID = u2Id,
                    ParkingLotID = lotId,
                    VehicleID = v2.ID
                }
            );

            await db.SaveChangesAsync();

            var service = new ReservationService(db);
            var res = (await service.GetReservationsForUserAsync("identity-u1")).ToList();

            Assert.Single(res);
            Assert.Equal(u1Id, res[0].UserID);
        }
    }

    public class ReservationControllerEndpointTests
    {
        private static ReservationController CreateControllerWithUser(IReservation service, string? identityUserId)
        {
            var controller = new ReservationController(service);

            if (identityUserId != null)
            {
                var user = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, identityUserId) },
                    "TestAuth"));

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                };
            }
            else
            {
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };
            }

            return controller;
        }

        [Fact]
        public async Task Create_ValidRequest_ReturnsCreated_201_WithReservationResponse()
        {
            var mock = new Mock<IReservation>();
            var req = new ReservationCreateRequest
            {
                LicensePlate = "TEST123",
                StartDate = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                ParkingLotId = Guid.NewGuid(),
                DiscountCode = "DISC10"
            };

            var createdModel = new Reservation
            {
                ID = Guid.NewGuid(),
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                Status = "Pending",
                CreatedAt = new DateTime(2025, 1, 1, 9, 0, 0, DateTimeKind.Utc),
                TotalPrice = 0f,
                UserID = Guid.NewGuid(),
                ParkingLotID = req.ParkingLotId,
                VehicleID = Guid.NewGuid(),
                DiscountCode = "DISC10"
            };

            mock.Setup(s => s.CreateReservationAsync(
                It.IsAny<ReservationCreateRequest>(),
                It.IsAny<string>()))
            .ReturnsAsync(createdModel);

            var controller = CreateControllerWithUser(mock.Object, "identity-1");

            var result = await controller.Create(req);

            var created = Assert.IsType<CreatedResult>(result);
            Assert.Equal(201, created.StatusCode);

            var response = Assert.IsType<ReservationResponse>(created.Value);
            Assert.Equal(createdModel.ID, response.Id);
            Assert.Equal(req.LicensePlate, response.LicensePlate);
            Assert.Equal(createdModel.VehicleID, response.VehicleId);
            Assert.Equal(createdModel.ParkingLotID, response.ParkingLotId);
            Assert.Equal(createdModel.Status, response.Status);
            Assert.Equal(createdModel.DiscountCode, response.DiscountCode);
        }

        [Fact]
        public async Task Create_ModelStateInvalid_ReturnsBadRequest_400()
        {
            var mock = new Mock<IReservation>();
            var controller = CreateControllerWithUser(mock.Object, "identity-1");
            controller.ModelState.AddModelError("x", "invalid");

            var req = new ReservationCreateRequest
            {
                LicensePlate = "",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddHours(1),
                ParkingLotId = Guid.NewGuid()
            };

            var result = await controller.Create(req);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, bad.StatusCode);
        }

        [Fact]
        public async Task Create_ServiceThrowsArgumentException_ReturnsBadRequest_400_WithError()
        {
            var mock = new Mock<IReservation>();
            mock.Setup(s => s.CreateReservationAsync(It.IsAny<ReservationCreateRequest>()))
                .ThrowsAsync(new ArgumentException("Parking lot not found."));

            var controller = CreateControllerWithUser(mock.Object, "identity-1");

            var req = new ReservationCreateRequest
            {
                LicensePlate = "X",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddHours(1),
                ParkingLotId = Guid.NewGuid()
            };

            var result = await controller.Create(req);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, bad.StatusCode);

            var errorProp = bad.Value!.GetType().GetProperty("error");
            Assert.NotNull(errorProp);
            Assert.Equal("Parking lot not found.", errorProp!.GetValue(bad.Value));
        }

        [Fact]
        public async Task Create_ReturnsCreated_LocationHeader_Contains_Id()
        {
            var mock = new Mock<IReservation>();
            var req = new ReservationCreateRequest
            {
                LicensePlate = "TEST123",
                StartDate = new DateTime(2025, 2, 1, 10, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2025, 2, 1, 11, 0, 0, DateTimeKind.Utc),
                ParkingLotId = Guid.NewGuid()
            };

            var createdModel = new Reservation
            {
                ID = Guid.NewGuid(),
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                TotalPrice = 0f,
                UserID = Guid.NewGuid(),
                ParkingLotID = req.ParkingLotId,
                VehicleID = Guid.NewGuid()
            };

            mock.Setup(s => s.CreateReservationAsync(It.IsAny<ReservationCreateRequest>()))
                .ReturnsAsync(createdModel);

            var controller = CreateControllerWithUser(mock.Object, "identity-1");

            var result = await controller.Create(req);

            var created = Assert.IsType<CreatedResult>(result);
            Assert.Contains(createdModel.ID.ToString(), created.Location);
        }

        [Fact]
        public async Task Create_CallsServiceOnce()
        {
            var mock = new Mock<IReservation>();

            mock.Setup(s => s.CreateReservationAsync(It.IsAny<ReservationCreateRequest>()))
                .ReturnsAsync(new Reservation
                {
                    ID = Guid.NewGuid(),
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddHours(1),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    TotalPrice = 0,
                    UserID = Guid.NewGuid(),
                    ParkingLotID = Guid.NewGuid(),
                    VehicleID = Guid.NewGuid()
                });

            var controller = CreateControllerWithUser(mock.Object, "identity-1");

            var req = new ReservationCreateRequest
            {
                LicensePlate = "TEST123",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddHours(1),
                ParkingLotId = Guid.NewGuid()
            };

            _ = await controller.Create(req);

            mock.Verify(s => s.CreateReservationAsync(It.IsAny<ReservationCreateRequest>()), Times.Once);
        }

        [Fact]
        public async Task GetForCurrentUser_Unauthorized_When_NoIdentityUserId_Returns401()
        {
            var mock = new Mock<IReservation>();
            var controller = CreateControllerWithUser(mock.Object, identityUserId: null);

            var result = await controller.GetForCurrentUser();

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(401, obj.StatusCode);
        }

        [Fact]
        public async Task GetForCurrentUser_ReturnsOk_200_WithMappedReservationResponses()
        {
            var mock = new Mock<IReservation>();
            var identityId = "identity-1";

            var res1 = new Reservation
            {
                ID = Guid.NewGuid(),
                StartDate = new DateTime(2025, 3, 1, 10, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2025, 3, 1, 12, 0, 0, DateTimeKind.Utc),
                Status = "Pending",
                CreatedAt = new DateTime(2025, 3, 1, 9, 0, 0, DateTimeKind.Utc),
                TotalPrice = 0,
                UserID = Guid.NewGuid(),
                ParkingLotID = Guid.NewGuid(),
                VehicleID = Guid.NewGuid(),
                DiscountCode = "DISC",
                Vehicle = new Vehicle { ID = Guid.NewGuid(), OldID = "", LicensePlate = "CAR1", UserID = Guid.NewGuid() }
            };

            var res2 = new Reservation
            {
                ID = Guid.NewGuid(),
                StartDate = new DateTime(2025, 3, 2, 10, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2025, 3, 2, 11, 0, 0, DateTimeKind.Utc),
                Status = "Pending",
                CreatedAt = new DateTime(2025, 3, 2, 9, 0, 0, DateTimeKind.Utc),
                TotalPrice = 0,
                UserID = Guid.NewGuid(),
                ParkingLotID = Guid.NewGuid(),
                VehicleID = Guid.NewGuid(),
                Vehicle = null
            };

            mock.Setup(s => s.GetReservationsForUserAsync(identityId))
                .ReturnsAsync(new List<Reservation> { res1, res2 });

            var controller = CreateControllerWithUser(mock.Object, identityId);

            var result = await controller.GetForCurrentUser();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);

            var list = Assert.IsAssignableFrom<IEnumerable<ReservationResponse>>(ok.Value);
            var arr = list.ToList();

            Assert.Equal(2, arr.Count);
            Assert.Equal("CAR1", arr[0].LicensePlate);
            Assert.Equal(string.Empty, arr[1].LicensePlate);
            Assert.Equal("DISC", arr[0].DiscountCode);
        }

        [Fact]
        public async Task GetForCurrentUser_ServiceThrowsArgumentException_ReturnsBadRequest_400()
        {
            var mock = new Mock<IReservation>();
            var identityId = "identity-1";

            mock.Setup(s => s.GetReservationsForUserAsync(identityId))
                .ThrowsAsync(new ArgumentException("User not found."));

            var controller = CreateControllerWithUser(mock.Object, identityId);

            var result = await controller.GetForCurrentUser();

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, bad.StatusCode);
        }

        [Fact]
        public async Task GetForCurrentUser_CallsServiceOnce_WithIdentity()
        {
            var mock = new Mock<IReservation>();
            var identityId = "identity-1";

            mock.Setup(s => s.GetReservationsForUserAsync(identityId))
                .ReturnsAsync(new List<Reservation>());

            var controller = CreateControllerWithUser(mock.Object, identityId);

            _ = await controller.GetForCurrentUser();

            mock.Verify(s => s.GetReservationsForUserAsync(identityId), Times.Once);
        }

        [Fact]
        public async Task GetForCurrentUser_ReturnsOk_EvenWhenEmptyList()
        {
            var mock = new Mock<IReservation>();
            var identityId = "identity-1";

            mock.Setup(s => s.GetReservationsForUserAsync(identityId))
                .ReturnsAsync(new List<Reservation>());

            var controller = CreateControllerWithUser(mock.Object, identityId);

            var result = await controller.GetForCurrentUser();

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<ReservationResponse>>(ok.Value);
            Assert.Empty(list);
        }

        [Fact]
        public async Task Update_ModelStateInvalid_ReturnsBadRequest_400()
        {
            var mock = new Mock<IReservation>();
            var controller = CreateControllerWithUser(mock.Object, "identity-1");
            controller.ModelState.AddModelError("x", "invalid");

            var result = await controller.Update(Guid.NewGuid(), new ReservationUpdateRequest());

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, bad.StatusCode);
        }

        [Fact]
        public async Task Update_Unauthorized_When_NoIdentityUserId_Returns401()
        {
            var mock = new Mock<IReservation>();
            var controller = CreateControllerWithUser(mock.Object, identityUserId: null);

            var result = await controller.Update(Guid.NewGuid(), new ReservationUpdateRequest());

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(401, obj.StatusCode);
        }

        [Fact]
        public async Task Update_Success_200_ReturnsOk_WithReservationResponse()
        {
            var mock = new Mock<IReservation>();
            var identityId = "identity-1";
            var reservationId = Guid.NewGuid();

            var updated = new Reservation
            {
                ID = reservationId,
                StartDate = new DateTime(2025, 4, 1, 10, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2025, 4, 1, 12, 0, 0, DateTimeKind.Utc),
                Status = "Pending",
                CreatedAt = new DateTime(2025, 4, 1, 9, 0, 0, DateTimeKind.Utc),
                TotalPrice = 0,
                UserID = Guid.NewGuid(),
                ParkingLotID = Guid.NewGuid(),
                VehicleID = Guid.NewGuid(),
                DiscountCode = "X",
                Vehicle = new Vehicle { ID = Guid.NewGuid(), OldID = "", LicensePlate = "UPD1", UserID = Guid.NewGuid() }
            };

            mock.Setup(s => s.UpdateReservationForUserAsync(reservationId, identityId, It.IsAny<ReservationUpdateRequest>()))
                .ReturnsAsync((updated, 200, new { message = "Reservation updated successfully." }));

            var controller = CreateControllerWithUser(mock.Object, identityId);

            var result = await controller.Update(reservationId, new ReservationUpdateRequest
            {
                StartDate = updated.StartDate,
                EndDate = updated.EndDate
            });

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);

            var response = Assert.IsType<ReservationResponse>(ok.Value);
            Assert.Equal(reservationId, response.Id);
            Assert.Equal("UPD1", response.LicensePlate);
            Assert.Equal("X", response.DiscountCode);
        }

        [Fact]
        public async Task Update_ServiceReturns400_Returns400_ObjectResult()
        {
            var mock = new Mock<IReservation>();
            var identityId = "identity-1";
            var reservationId = Guid.NewGuid();

            mock.Setup(s => s.UpdateReservationForUserAsync(reservationId, identityId, It.IsAny<ReservationUpdateRequest>()))
                .ReturnsAsync((null, 400, new { error = "At least one field must be provided to update." }));

            var controller = CreateControllerWithUser(mock.Object, identityId);

            var result = await controller.Update(reservationId, new ReservationUpdateRequest());

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, obj.StatusCode);
        }

        [Fact]
        public async Task Update_ServiceReturns404_Returns404_ObjectResult()
        {
            var mock = new Mock<IReservation>();
            var identityId = "identity-1";
            var reservationId = Guid.NewGuid();

            mock.Setup(s => s.UpdateReservationForUserAsync(reservationId, identityId, It.IsAny<ReservationUpdateRequest>()))
                .ReturnsAsync((null, 404, new { error = "Reservation not found or not owned by current user." }));

            var controller = CreateControllerWithUser(mock.Object, identityId);

            var result = await controller.Update(reservationId, new ReservationUpdateRequest { StartDate = DateTime.UtcNow.AddDays(1) });

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, obj.StatusCode);
        }

        [Fact]
        public async Task Update_ServiceReturns500_Returns500_ObjectResult()
        {
            var mock = new Mock<IReservation>();
            var identityId = "identity-1";
            var reservationId = Guid.NewGuid();

            mock.Setup(s => s.UpdateReservationForUserAsync(reservationId, identityId, It.IsAny<ReservationUpdateRequest>()))
                .ReturnsAsync((null, 500, new { error = "An unexpected error occurred." }));

            var controller = CreateControllerWithUser(mock.Object, identityId);

            var result = await controller.Update(reservationId, new ReservationUpdateRequest { StartDate = DateTime.UtcNow.AddDays(1) });

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task Update_UnhandledStatusCode_Returns501_WithErrorObject()
        {
            var mock = new Mock<IReservation>();
            var identityId = "identity-1";
            var reservationId = Guid.NewGuid();

            mock.Setup(s => s.UpdateReservationForUserAsync(reservationId, identityId, It.IsAny<ReservationUpdateRequest>()))
                .ReturnsAsync((null, 418, new { error = "teapot" }));

            var controller = CreateControllerWithUser(mock.Object, identityId);

            var result = await controller.Update(reservationId, new ReservationUpdateRequest { StartDate = DateTime.UtcNow.AddDays(1) });

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(501, obj.StatusCode);

            var errProp = obj.Value!.GetType().GetProperty("error");
            Assert.NotNull(errProp);
            var text = errProp!.GetValue(obj.Value) as string;
            Assert.Contains("Unhandled statuscode: 418", text);
        }

        [Fact]
        public async Task Delete_Unauthorized_When_NoIdentityUserId_Returns401()
        {
            var mock = new Mock<IReservation>();
            var controller = CreateControllerWithUser(mock.Object, identityUserId: null);

            var result = await controller.Delete(Guid.NewGuid());

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(401, obj.StatusCode);
        }

        [Fact]
        public async Task Delete_WhenServiceReturnsFalse_Returns404_NotFound()
        {
            var mock = new Mock<IReservation>();
            var identityId = "identity-1";
            var reservationId = Guid.NewGuid();

            mock.Setup(s => s.DeleteReservationForUserAsync(reservationId, identityId))
                .ReturnsAsync(false);

            var controller = CreateControllerWithUser(mock.Object, identityId);

            var result = await controller.Delete(reservationId);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFound.StatusCode);
        }

        [Fact]
        public async Task Delete_WhenServiceReturnsTrue_Returns204_NoContent()
        {
            var mock = new Mock<IReservation>();
            var identityId = "identity-1";
            var reservationId = Guid.NewGuid();

            mock.Setup(s => s.DeleteReservationForUserAsync(reservationId, identityId))
                .ReturnsAsync(true);

            var controller = CreateControllerWithUser(mock.Object, identityId);

            var result = await controller.Delete(reservationId);

            var no = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, no.StatusCode);
        }

        [Fact]
        public async Task Delete_CallsServiceOnce_WithCorrectArgs()
        {
            var mock = new Mock<IReservation>();
            var identityId = "identity-1";
            var reservationId = Guid.NewGuid();

            mock.Setup(s => s.DeleteReservationForUserAsync(reservationId, identityId))
                .ReturnsAsync(true);

            var controller = CreateControllerWithUser(mock.Object, identityId);

            _ = await controller.Delete(reservationId);

            mock.Verify(s => s.DeleteReservationForUserAsync(reservationId, identityId), Times.Once);
        }

        [Fact]
        public async Task Delete_NotFound_ReturnsExpectedErrorShape()
        {
            var mock = new Mock<IReservation>();
            var identityId = "identity-1";
            var reservationId = Guid.NewGuid();

            mock.Setup(s => s.DeleteReservationForUserAsync(reservationId, identityId))
                .ReturnsAsync(false);

            var controller = CreateControllerWithUser(mock.Object, identityId);

            var result = await controller.Delete(reservationId);

            var nf = Assert.IsType<NotFoundObjectResult>(result);

            var errorProp = nf.Value!.GetType().GetProperty("error");
            Assert.NotNull(errorProp);

            var msg = errorProp!.GetValue(nf.Value) as string;
            Assert.Equal("Reservation not found or not owned by current user.", msg);
        }
    }
}
