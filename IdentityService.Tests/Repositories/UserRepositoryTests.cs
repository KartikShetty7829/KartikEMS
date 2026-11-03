using FluentAssertions;
using IdentityService.Entities;
using IdentityService.Tests.TestHelpers;
using Xunit;

namespace IdentityService.Tests.Repositories
{
    public class UserRepositoryTests
    {
        [Fact]
        public async Task GetAdminUserByUsernameAsync_WhenAdminUserExists_ReturnsUserWithRole()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var adminRole = new Role { RoleId = Guid.NewGuid(), Name = "Admin" };
            context.Roles.Add(adminRole);

            var adminUser = new User
            {
                UserId = Guid.NewGuid(),
                Username = "superadmin",
                Password = "hashedpassword",
                RoleId = adminRole.RoleId,
                Role = adminRole,
                IsActive = true
            };
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();

            var userRepo = DbContextHelper.CreateUserRepository(context);

            // Act
            var result = await userRepo.GetAdminUserByUsernameAsync("superadmin", CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Username.Should().Be("superadmin");
            result.Role.Should().NotBeNull();
            result.Role.Name.Should().Be("Admin");
        }

        [Fact]
        public async Task GetAdminUserByUsernameAsync_WhenUserIsEmployee_ReturnsNull()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var employeeRole = new Role { RoleId = Guid.NewGuid(), Name = "Employee" };
            context.Roles.Add(employeeRole);

            var empUser = new User
            {
                UserId = Guid.NewGuid(),
                Username = "employee1",
                Password = "hashedpassword",
                RoleId = employeeRole.RoleId,
                Role = employeeRole,
                IsActive = true
            };
            context.Users.Add(empUser);
            await context.SaveChangesAsync();

            var userRepo = DbContextHelper.CreateUserRepository(context);

            // Act
            var result = await userRepo.GetAdminUserByUsernameAsync("employee1", CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAdminUserByUsernameAsync_WhenUserDoesNotExist_ReturnsNull()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var userRepo = DbContextHelper.CreateUserRepository(context);

            // Act
            var result = await userRepo.GetAdminUserByUsernameAsync("nonexistent", CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
}
