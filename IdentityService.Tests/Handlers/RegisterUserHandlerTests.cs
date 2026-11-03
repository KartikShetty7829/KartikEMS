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
    public class RegisterUserHandlerTests
    {
        private readonly Mock<IPasswordHasherService> _mockPasswordHasher;

        public RegisterUserHandlerTests()
        {
            _mockPasswordHasher = new Mock<IPasswordHasherService>();
        }

        [Fact]
        public async Task Handle_ValidRequest_CreatesUserWithHashedPasswordAndReturnsSuccess()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var role = new Role { RoleId = Guid.NewGuid(), Name = "Employee" };
            context.Roles.Add(role);
            await context.SaveChangesAsync();

            var userRepo = DbContextHelper.CreateRepository<User>(context);
            var roleRepo = DbContextHelper.CreateRepository<Role>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<RegisterUserHandler>();

            _mockPasswordHasher
                .Setup(p => p.HashPassword("PlainPassword123"))
                .Returns("hashed.10000.salt.key");

            var handler = new RegisterUserHandler(userRepo, roleRepo, logger, unitOfWork, _mockPasswordHasher.Object);

            var request = new RegisterUserRequestDto
            {
                Username = "new_user",
                Password = "PlainPassword123",
                EmployeeNumber = 3001,
                Role = "Employee"
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Username.Should().Be("new_user");
            result.Role.Should().Be("Employee");
            result.EmployeeNumber.Should().Be(3001);
            result.Message.Should().Be("User 'new_user' registered successfully with role 'Employee'.");

            var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "new_user");
            savedUser.Should().NotBeNull();
            savedUser!.Password.Should().Be("hashed.10000.salt.key");
            savedUser.RoleId.Should().Be(role.RoleId);
            savedUser.EmployeeNumber.Should().Be(3001);
        }

        [Fact]
        public async Task Handle_UsernameAlreadyExists_ThrowsBadRequestException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var role = new Role { RoleId = Guid.NewGuid(), Name = "Employee" };
            context.Roles.Add(role);
            context.Users.Add(new User
            {
                UserId = Guid.NewGuid(),
                Username = "existing_user",
                Password = "password",
                RoleId = role.RoleId,
                EmployeeNumber = 3001
            });
            await context.SaveChangesAsync();

            var userRepo = DbContextHelper.CreateRepository<User>(context);
            var roleRepo = DbContextHelper.CreateRepository<Role>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<RegisterUserHandler>();

            var handler = new RegisterUserHandler(userRepo, roleRepo, logger, unitOfWork, _mockPasswordHasher.Object);

            var request = new RegisterUserRequestDto
            {
                Username = "existing_user",
                Password = "anyPassword",
                EmployeeNumber = 3002,
                Role = "Employee"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("User 'existing_user' already exists.");
        }

        [Fact]
        public async Task Handle_EmployeeNumberAlreadyHasAccount_ThrowsBadRequestException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var role = new Role { RoleId = Guid.NewGuid(), Name = "Employee" };
            context.Roles.Add(role);
            context.Users.Add(new User
            {
                UserId = Guid.NewGuid(),
                Username = "first_user",
                Password = "password",
                RoleId = role.RoleId,
                EmployeeNumber = 3001
            });
            await context.SaveChangesAsync();

            var userRepo = DbContextHelper.CreateRepository<User>(context);
            var roleRepo = DbContextHelper.CreateRepository<Role>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<RegisterUserHandler>();

            var handler = new RegisterUserHandler(userRepo, roleRepo, logger, unitOfWork, _mockPasswordHasher.Object);

            var request = new RegisterUserRequestDto
            {
                Username = "second_user",
                Password = "anyPassword",
                EmployeeNumber = 3001,
                Role = "Employee"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("EmployeeNumber '3001' already has a user account.");
        }

        [Fact]
        public async Task Handle_RoleNotFound_ThrowsNotFoundException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var userRepo = DbContextHelper.CreateRepository<User>(context);
            var roleRepo = DbContextHelper.CreateRepository<Role>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<RegisterUserHandler>();

            var handler = new RegisterUserHandler(userRepo, roleRepo, logger, unitOfWork, _mockPasswordHasher.Object);

            var request = new RegisterUserRequestDto
            {
                Username = "new_user",
                Password = "password",
                EmployeeNumber = 3001,
                Role = "NonExistentRole"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Role 'NonExistentRole' not found.");
        }
    }
}
