using FluentAssertions;
using IdentityService.Business_Handler;
using IdentityService.Data;
using IdentityService.Domain;
using IdentityService.Entities;
using IdentityService.Exceptions;
using IdentityService.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IdentityService.Tests.Handlers
{
    public class ForgotPasswordHandlerTests
    {
        [Fact]
        public async Task Handle_ValidUser_UpdatesPasswordAndReturnsSuccess()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var role = new Role { RoleId = Guid.NewGuid(), Name = "Employee" };
            context.Roles.Add(role);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "user_to_reset",
                Password = "old_password",
                RoleId = role.RoleId,
                EmployeeNumber = 4001
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userRepo = DbContextHelper.CreateRepository<User>(context);
            var logger = DbContextHelper.CreateLogger<ForgotPasswordHandler>();

            var handler = new ForgotPasswordHandler(userRepo, logger);

            var request = new ForgotPasswordRequestDto
            {
                Username = "user_to_reset",
                NewPassword = "brand_new_password"
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Password reset successfully.");

            var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "user_to_reset");
            updatedUser.Should().NotBeNull();
            updatedUser!.Password.Should().Be("brand_new_password");
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsNotFoundException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var userRepo = DbContextHelper.CreateRepository<User>(context);
            var logger = DbContextHelper.CreateLogger<ForgotPasswordHandler>();

            var handler = new ForgotPasswordHandler(userRepo, logger);

            var request = new ForgotPasswordRequestDto
            {
                Username = "non_existent_user",
                NewPassword = "new_password"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("User not found.");
        }
    }
}
