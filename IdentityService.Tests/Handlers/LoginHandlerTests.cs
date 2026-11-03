using FluentAssertions;
using IdentityService.Business_Handler;
using IdentityService.Data;
using IdentityService.Domain;
using IdentityService.Entities;
using IdentityService.Exceptions;
using IdentityService.Interface;
using IdentityService.Services;
using IdentityService.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace IdentityService.Tests.Handlers
{
    public class LoginHandlerTests
    {
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly Mock<IPasswordHasherService> _mockPasswordHasher;

        public LoginHandlerTests()
        {
            _mockJwtService = new Mock<IJwtService>();
            _mockPasswordHasher = new Mock<IPasswordHasherService>();
        }

        [Fact]
        public async Task Handle_ValidEmployeeCredentials_ReturnsSuccessAndToken()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var employeeRole = new Role { RoleId = Guid.NewGuid(), Name = "Employee" };
            context.Roles.Add(employeeRole);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "emp_john",
                Password = "hashed_emp_password",
                RoleId = employeeRole.RoleId,
                Role = employeeRole,
                IsActive = true,
                EmployeeNumber = 1001
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userRepo = DbContextHelper.CreateRepository<User>(context);
            var logger = DbContextHelper.CreateLogger<LoginHandler>();

            _mockPasswordHasher
                .Setup(p => p.VerifyPassword("plainPassword", "hashed_emp_password"))
                .Returns(true);

            _mockJwtService
                .Setup(j => j.GenerateJwtToken(It.Is<User>(u => u.UserId == user.UserId), "Employee"))
                .Returns("jwt.employee.token");

            var handler = new LoginHandler(userRepo, _mockJwtService.Object, logger, _mockPasswordHasher.Object);

            var request = new LoginRequestDto
            {
                Username = "emp_john",
                Password = "plainPassword"
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Employee login successful.");
            result.Token.Should().Be("jwt.employee.token");
            result.Role.Should().Be("Employee");
            result.Username.Should().Be("emp_john");
            result.EmployeeNumber.Should().Be(1001);
            result.UserId.Should().Be(user.UserId);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsUnauthorizedException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var userRepo = DbContextHelper.CreateRepository<User>(context);
            var logger = DbContextHelper.CreateLogger<LoginHandler>();

            var handler = new LoginHandler(userRepo, _mockJwtService.Object, logger, _mockPasswordHasher.Object);

            var request = new LoginRequestDto
            {
                Username = "nonexistent_emp",
                Password = "password"
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
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var employeeRole = new Role { RoleId = Guid.NewGuid(), Name = "Employee" };
            context.Roles.Add(employeeRole);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "emp_john",
                Password = "hashed_emp_password",
                RoleId = employeeRole.RoleId,
                Role = employeeRole,
                IsActive = true
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userRepo = DbContextHelper.CreateRepository<User>(context);
            var logger = DbContextHelper.CreateLogger<LoginHandler>();

            _mockPasswordHasher
                .Setup(p => p.VerifyPassword("wrongPassword", "hashed_emp_password"))
                .Returns(false);

            var handler = new LoginHandler(userRepo, _mockJwtService.Object, logger, _mockPasswordHasher.Object);

            var request = new LoginRequestDto
            {
                Username = "emp_john",
                Password = "wrongPassword"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedException>()
                .WithMessage("Invalid username or password.");
        }

        [Fact]
        public async Task Handle_InactiveUser_ThrowsUnauthorizedException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var employeeRole = new Role { RoleId = Guid.NewGuid(), Name = "Employee" };
            context.Roles.Add(employeeRole);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "inactive_emp",
                Password = "hashed_password",
                RoleId = employeeRole.RoleId,
                Role = employeeRole,
                IsActive = false
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userRepo = DbContextHelper.CreateRepository<User>(context);
            var logger = DbContextHelper.CreateLogger<LoginHandler>();

            _mockPasswordHasher
                .Setup(p => p.VerifyPassword("password", "hashed_password"))
                .Returns(true);

            var handler = new LoginHandler(userRepo, _mockJwtService.Object, logger, _mockPasswordHasher.Object);

            var request = new LoginRequestDto
            {
                Username = "inactive_emp",
                Password = "password"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedException>()
                .WithMessage("Your account is inactive. you resigned from this company .");
        }

        [Fact]
        public async Task Handle_NonEmployeeRole_ThrowsUnauthorizedException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var adminRole = new Role { RoleId = Guid.NewGuid(), Name = "Admin" };
            context.Roles.Add(adminRole);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "admin_trying_employee_login",
                Password = "hashed_password",
                RoleId = adminRole.RoleId,
                Role = adminRole,
                IsActive = true
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userRepo = DbContextHelper.CreateRepository<User>(context);
            var logger = DbContextHelper.CreateLogger<LoginHandler>();

            _mockPasswordHasher
                .Setup(p => p.VerifyPassword("password", "hashed_password"))
                .Returns(true);

            var handler = new LoginHandler(userRepo, _mockJwtService.Object, logger, _mockPasswordHasher.Object);

            var request = new LoginRequestDto
            {
                Username = "admin_trying_employee_login",
                Password = "password"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedException>()
                .WithMessage("You are not authorized as employee.");
        }
    }
}
