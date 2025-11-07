using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Moq;
using v2.infrastructure.Services;
using v2.Infrastructure.Data;
using v2.Core.DTOs;
using v2.Core.Models;
using System.Collections.Generic;

namespace v2.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly Mock<SignInManager<IdentityUser>> _signInManagerMock;
        private readonly ApplicationDbContext _dbContext;
        private readonly AuthService _authService;
        private readonly Mock<IPasswordHasher<IdentityUser>> _passwordHasherMock;

        public AuthServiceTests()
        {
            
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);

            var userStoreMock = new Mock<IUserStore<IdentityUser>>();

            _passwordHasherMock = new Mock<IPasswordHasher<IdentityUser>>();

            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object, null, _passwordHasherMock.Object, null, null, null, null, null, null
            );
            var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var userClaimsPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            _signInManagerMock = new Mock<SignInManager<IdentityUser>>(
                _userManagerMock.Object,
                contextAccessor.Object,
                userClaimsPrincipalFactory.Object,
                null, null, null, null
            );

            _authService = new AuthService(_dbContext, _signInManagerMock.Object, _userManagerMock.Object);
        }

        [Fact]
        public async Task RegisterUser_Success()
        {
            
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Password = "Password123!",
                Name = "Test User"
            };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { Id = "123", UserName = "testuser" });

            
            var result = await _authService.RegisterUser(registerDto);

            
            Assert.Equal(201, result.statusCode);
            Assert.NotNull(result.data);
            Assert.Equal("testuser", result.data.Username);
            Assert.Equal("Test User", result.data.Name);
            

            var userInDb = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
            Assert.NotNull(userInDb);
        }


        [Fact]
        public async Task LoginUser_Success()
        {
            
            var loginDto = new LoginDto
            {
                username = "testuser",
                password = "Password123!"
            };

            var user = new IdentityUser { Id = "123", UserName = "testuser" };
            _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, loginDto.password, false))
                .ReturnsAsync(SignInResult.Success);

           
            var result = await _authService.LoginUser(loginDto);

         
            Assert.Equal(200, result.statusCode);
            Assert.NotNull(result.message);
            Assert.Contains("accesstoken", result.message.ToString());
        }
        
        [Fact]
        public async Task LoginUser_UserNotFound()
        {
          
            var loginDto = new LoginDto { username = "missing", password = "wrong" };
            _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((IdentityUser)null);

            var result = await _authService.LoginUser(loginDto);

            Assert.Equal(404, result.statusCode);
            Assert.Contains("User not found", result.message.ToString());
        }

        [Fact]
        public async Task GetProfile_Success()
        {
            var guid = Guid.NewGuid();
            IdentityUser identityUser = new IdentityUser
            {
                Id = "abc123",
                UserName = "john",
                PasswordHash = "hashedpassword"
            };
            var user = new User
            {
                ID = guid,
                IdentityUserId = "abc123",
                IdentityUser = identityUser,
                Username = "john",
                Name = "John Doe",
                Email = "john@example.com",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Vehicles = null,
                Sessions = null,
                Reservations = null
            };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            _userManagerMock.Setup(x => x.FindByIdAsync("abc123"))
                .ReturnsAsync(new IdentityUser { Id = "abc123", PasswordHash = "hashed" });

            var result = await _authService.GetProfile("abc123");

            Assert.Equal(200, result.statusCode);
            Assert.NotNull(result.data);
            Assert.Equal("john", result.data.Username);
        }

        [Fact]
        public async Task GetProfile_NotFound()
        {
            var result = await _authService.GetProfile("does-not-exist");

            Assert.Equal(404, result.statusCode);
            Assert.Contains("User not found", result.message.ToString());
        }

        [Fact]
        public async Task RegisterUser_FailsOnCreate()
        {
            var registerDto = new RegisterDto { Username = "failuser", Password = "Password123!", Name = "Fail User" };
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid password" }));

            var result = await _authService.RegisterUser(registerDto);

            Assert.Equal(400, result.statusCode);
            Assert.Null(result.data);
            Assert.Contains("error", result.message.ToString());
        }

        

        [Fact]
        public async Task LoginUser_InvalidPassword()
        {
            var user = new IdentityUser { Id = "123", UserName = "testuser" };
            _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, "wrong", false))
                .ReturnsAsync(SignInResult.Failed);

            var result = await _authService.LoginUser(new LoginDto { username = "testuser", password = "wrong" });

            Assert.Equal(401, result.statusCode);
            Assert.Contains("Invalid credentials", result.message.ToString());
        }



        [Fact]
        public async Task UpdateProfile_UserNotFound()
        {
            Guid id = Guid.NewGuid();
            var profileDto = new ProfileDto { Id = id, Username = "ghost", Password = "pass" };
            var result = await _authService.UpdateProfile(profileDto, "nonexistent");
            Console.WriteLine($"Hellol{result.message.ToString()}");
            Assert.Equal(404, result.statusCode);
            Assert.Contains("error", result.message.ToString());
        }
        
        [Fact]
        public async Task UpdateProfile_Success_PasswordAndProfile()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            var identityUser = new IdentityUser { Id = "123", UserName = "olduser", Email = "old@example.com", PasswordHash = "oldhash" };
            var user = new User {
                ID = id,
                IdentityUserId = "123",
                IdentityUser = identityUser,
                Username = "olduser",
                Name = "Old Name",
                Email = "old@example.com",
                IsActive = true,
                Vehicles = new List<Vehicle>(),
                Sessions = new List<Session>(),
                Reservations = new List<Reservation>()
            };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            
            var dto = new ProfileDto
            {
                Id = id,
                Username = "newuser",
                Password = "NewPassword123!",
                Email = "new@example.com",
                Name = "New Name",
                Role = "Admin",
                Active = true,
                Birth_year = 1990,
                Created_at = DateTime.UtcNow
            };

            _userManagerMock.Setup(x => x.FindByIdAsync("123")).ReturnsAsync(identityUser);
            _passwordHasherMock
                .Setup(h => h.VerifyHashedPassword(identityUser, "oldhash", "NewPassword123!"))
                .Returns(PasswordVerificationResult.Failed);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(identityUser)).ReturnsAsync("token");
            _userManagerMock.Setup(x => x.ResetPasswordAsync(identityUser, "token", dto.Password))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.SetEmailAsync(identityUser, dto.Email)).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.SetUserNameAsync(identityUser, dto.Username)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.UpdateProfile(dto, "123");

            // Assert
            Assert.Equal(200, result.statusCode);
            Assert.Contains("Profile updated successfully", result.message.ToString());
            
            var updatedUser = await _dbContext.Users.FirstAsync(u => u.ID == id);
            Assert.Equal("newuser", updatedUser.Username);
            Assert.Equal("New Name", updatedUser.Name);
            Assert.Equal("new@example.com", updatedUser.Email);
        }
    }
}