using FluentAssertions;
using IdentityService.Business_Handler;
using IdentityService.Domain;
using IdentityService.Entities;
using IdentityService.Exceptions;
using IdentityService.Interface;
using IdentityService.Services;
using IdentityService.Tests.TestHelpers;
using Moq;
using Xunit;

namespace IdentityService.Tests.Handlers
{
    public class AdminLoginHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly Mock<IPasswordHasherService> _mockPasswordHasher;

        public AdminLoginHandlerTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockJwtService = new Mock<IJwtService>();
            _mockPasswordHasher = new Mock<IPasswordHasherService>();
        }

        [Fact]
        public async Task Handle_ValidAdminCredentials_ReturnsSuccessAndToken()
        {
            // Arrange
            var adminRole = new Role { RoleId = Guid.NewGuid(), Name = "Admin" };
            var adminUser = new User
            {
                UserId = Guid.NewGuid(),
                Username = "adminUser",
                Password = "hashed_password",
                RoleId = adminRole.RoleId,
                Role = adminRole,
                IsActive = true,
                EmployeeNumber = 101
            };

            _mockUserRepo
                .Setup(r => r.GetAdminUserByUsernameAsync("adminUser", It.IsAny<CancellationToken>()))
                .ReturnsAsync(adminUser);

            _mockPasswordHasher
                .Setup(p => p.VerifyPassword("adminPass123", "hashed_password"))
                .Returns(true);

            _mockJwtService
                .Setup(j => j.GenerateJwtToken(adminUser, "Admin"))
                .Returns("jwt.mock.token");

            var logger = DbContextHelper.CreateLogger<AdminLoginHandler>();
            var handler = new AdminLoginHandler(_mockUserRepo.Object, _mockJwtService.Object, logger, _mockPasswordHasher.Object);

            var request = new AdminLoginRequestDto
            {
                Username = "adminUser",
                Password = "adminPass123"
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Admin login successful.");
            result.Token.Should().Be("jwt.mock.token");
            result.Role.Should().Be("Admin");
            result.Username.Should().Be("adminUser");
            result.AdminUserId.Should().Be(adminUser.UserId);
            result.EmployeeNumber.Should().Be(101);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsUnauthorizedException()
        {
            // Arrange
            _mockUserRepo
                .Setup(r => r.GetAdminUserByUsernameAsync("nonExistent", It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var logger = DbContextHelper.CreateLogger<AdminLoginHandler>();
            var handler = new AdminLoginHandler(_mockUserRepo.Object, _mockJwtService.Object, logger, _mockPasswordHasher.Object);

            var request = new AdminLoginRequestDto
            {
                Username = "nonExistent",
                Password = "anyPassword"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedException>()
                .WithMessage("Invalid username or password.");
        }

        [Fact]
        public async Task Handle_InvalidPassword_ThrowsUnauthorizedException()
        {
            // Arrange
            var adminRole = new Role { RoleId = Guid.NewGuid(), Name = "Admin" };
            var adminUser = new User
            {
                UserId = Guid.NewGuid(),
                Username = "adminUser",
                Password = "hashed_password",
                RoleId = adminRole.RoleId,
                Role = adminRole,
                IsActive = true
            };

            _mockUserRepo
                .Setup(r => r.GetAdminUserByUsernameAsync("adminUser", It.IsAny<CancellationToken>()))
                .ReturnsAsync(adminUser);

            _mockPasswordHasher
                .Setup(p => p.VerifyPassword("wrongPassword", "hashed_password"))
                .Returns(false);

            var logger = DbContextHelper.CreateLogger<AdminLoginHandler>();
            var handler = new AdminLoginHandler(_mockUserRepo.Object, _mockJwtService.Object, logger, _mockPasswordHasher.Object);

            var request = new AdminLoginRequestDto
            {
                Username = "adminUser",
                Password = "wrongPassword"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedException>()
                .WithMessage("Invalid username or password.");
        }

        [Fact]
        public async Task Handle_InactiveAdminUser_ThrowsUnauthorizedException()
        {
            // Arrange
            var adminRole = new Role { RoleId = Guid.NewGuid(), Name = "Admin" };
            var inactiveAdmin = new User
            {
                UserId = Guid.NewGuid(),
                Username = "inactiveAdmin",
                Password = "hashed_password",
                RoleId = adminRole.RoleId,
                Role = adminRole,
                IsActive = false
            };

            _mockUserRepo
                .Setup(r => r.GetAdminUserByUsernameAsync("inactiveAdmin", It.IsAny<CancellationToken>()))
                .ReturnsAsync(inactiveAdmin);

            _mockPasswordHasher
                .Setup(p => p.VerifyPassword("password", "hashed_password"))
                .Returns(true);

            var logger = DbContextHelper.CreateLogger<AdminLoginHandler>();
            var handler = new AdminLoginHandler(_mockUserRepo.Object, _mockJwtService.Object, logger, _mockPasswordHasher.Object);

            var request = new AdminLoginRequestDto
            {
                Username = "inactiveAdmin",
                Password = "password"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedException>()
                .WithMessage("Your admin account is inactive. you resigned from this Company.");
        }

        [Fact]
        public async Task Handle_UserRoleNotAdmin_ThrowsUnauthorizedException()
        {
            // Arrange
            var employeeRole = new Role { RoleId = Guid.NewGuid(), Name = "Employee" };
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "regularUser",
                Password = "hashed_password",
                RoleId = employeeRole.RoleId,
                Role = employeeRole,
                IsActive = true
            };

            _mockUserRepo
                .Setup(r => r.GetAdminUserByUsernameAsync("regularUser", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockPasswordHasher
                .Setup(p => p.VerifyPassword("password", "hashed_password"))
                .Returns(true);

            var logger = DbContextHelper.CreateLogger<AdminLoginHandler>();
            var handler = new AdminLoginHandler(_mockUserRepo.Object, _mockJwtService.Object, logger, _mockPasswordHasher.Object);

            var request = new AdminLoginRequestDto
            {
                Username = "regularUser",
                Password = "password"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedException>()
                .WithMessage("Access denied. You are not authorized as Admin.");
        }

        [Fact]
        public async Task Handle_UserRoleNull_ThrowsUnauthorizedException()
        {
            // Arrange
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "noRoleUser",
                Password = "hashed_password",
                Role = null!,
                IsActive = true
            };

            _mockUserRepo
                .Setup(r => r.GetAdminUserByUsernameAsync("noRoleUser", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockPasswordHasher
                .Setup(p => p.VerifyPassword("password", "hashed_password"))
                .Returns(true);

            var logger = DbContextHelper.CreateLogger<AdminLoginHandler>();
            var handler = new AdminLoginHandler(_mockUserRepo.Object, _mockJwtService.Object, logger, _mockPasswordHasher.Object);

            var request = new AdminLoginRequestDto
            {
                Username = "noRoleUser",
                Password = "password"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedException>()
                .WithMessage("Access denied. You are not authorized as Admin.");
        }
    }
}
