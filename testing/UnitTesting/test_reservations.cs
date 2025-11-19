using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
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


        [Fact]
        public async Task CreateReservation_Should_Fail_When_EndDate_Not_After_StartDate()
        {
            using var db = CreateInMemoryDbContext();
            var lotId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var vehicleId = Guid.NewGuid();

            db.ParkingLots.Add(CreateTestParkingLot(lotId));
            db.Vehicles.Add(CreateTestVehicle(vehicleId, userId, "TEST-123"));
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

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateReservationAsync(req));

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

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateReservationAsync(req));

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

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateReservationAsync(req));

            Assert.Contains("Vehicle with given license plate not found", ex.Message);
        }

        [Fact]
        public async Task CreateReservation_Should_Create_When_Valid()
        {
            using var db = CreateInMemoryDbContext();
            var lotId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var vehicleId = Guid.NewGuid();
            
            const string plate = "VALID1"; 

            db.ParkingLots.Add(CreateTestParkingLot(lotId));
            db.Vehicles.Add(CreateTestVehicle(vehicleId, userId, plate));
            await db.SaveChangesAsync();

            var service = new ReservationService(db);
            var start = DateTime.UtcNow.AddHours(1);
            var end = DateTime.UtcNow.AddHours(3);

            var req = new ReservationCreateRequest
            {
                LicensePlate = plate,
                StartDate = start,
                EndDate = end,
                ParkingLotId = lotId
            };

            var created = await service.CreateReservationAsync(req);
            var saved = await db.Reservations.SingleAsync(r => r.ID == created.ID);

            Assert.Equal("Pending", saved.Status);
            Assert.Equal(vehicleId, saved.VehicleID);
            Assert.Equal(lotId, saved.ParkingLotID);
            Assert.Equal(userId, saved.UserID);
            Assert.Equal(start, saved.StartDate);
            Assert.Equal(end, saved.EndDate);
        }

    public class ReservationControllerTests
    {
        [Fact]
        public async Task Create_ValidRequest_ReturnsCreatedWithReservationResponse()
        {
            var mockService = new Mock<IReservation>();

            var request = new ReservationCreateRequest
            {
                LicensePlate = "TEST123",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddHours(1),
                ParkingLotId = Guid.NewGuid()
            };

            var reservation = new Reservation
            {
                ID = Guid.NewGuid(),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                TotalPrice = 0f,
                UserID = Guid.NewGuid(),
                ParkingLotID = request.ParkingLotId,
                VehicleID = Guid.NewGuid()
            };

            mockService
                .Setup(s => s.CreateReservationAsync(request))
                .ReturnsAsync(reservation);

            var controller = new ReservationController(mockService.Object);

            var result = await controller.Create(request);

            var created = Assert.IsType<CreatedResult>(result);
            Assert.Equal(201, created.StatusCode);

            var response = Assert.IsType<ReservationResponse>(created.Value);
            Assert.Equal(reservation.ID, response.Id);
            Assert.Equal(request.LicensePlate, response.LicensePlate);
            Assert.Equal(reservation.ParkingLotID, response.ParkingLotId);
            Assert.Equal(reservation.Status, response.Status);
        }

        [Fact]
        public async Task Create_ModelStateInvalid_ReturnsBadRequest()
        {
            var mockService = new Mock<IReservation>();
            var controller = new ReservationController(mockService.Object);

            controller.ModelState.AddModelError("LicensePlate", "Required");

            var request = new ReservationCreateRequest
            {
                LicensePlate = "",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddHours(1),
                ParkingLotId = Guid.NewGuid()
            };

            var result = await controller.Create(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequest.StatusCode);
        }

        [Fact]
        public async Task Create_ServiceThrowsArgumentException_ReturnsBadRequest()
        {
            var mockService = new Mock<IReservation>();

            var request = new ReservationCreateRequest
            {
                LicensePlate = "INVALID",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddHours(1),
                ParkingLotId = Guid.NewGuid()
            };

            mockService
                .Setup(s => s.CreateReservationAsync(It.IsAny<ReservationCreateRequest>()))
                .ThrowsAsync(new ArgumentException("Vehicle with given license plate not found."));

            var controller = new ReservationController(mockService.Object);

            var result = await controller.Create(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequest.StatusCode);
        }
    }
}
}