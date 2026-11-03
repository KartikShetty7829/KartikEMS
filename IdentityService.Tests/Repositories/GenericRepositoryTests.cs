using FluentAssertions;
using IdentityService.Entities;
using IdentityService.Interface;
using IdentityService.Tests.TestHelpers;
using Xunit;

namespace IdentityService.Tests.Repositories
{
    public class GenericRepositoryTests
    {
        [Fact]
        public async Task AddAsync_AddsRoleSuccessfully()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var repo = DbContextHelper.CreateRepository<Role>(context);
            var role = new Role { RoleId = Guid.NewGuid(), Name = "Manager" };

            // Act
            await repo.AddAsync(role);
            await context.SaveChangesAsync();

            // Assert
            var saved = await context.Roles.FindAsync(role.RoleId);
            saved.Should().NotBeNull();
            saved!.Name.Should().Be("Manager");
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsEntity()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var roleId = Guid.NewGuid();
            var role = new Role { RoleId = roleId, Name = "HR" };
            context.Roles.Add(role);
            await context.SaveChangesAsync();

            var repo = DbContextHelper.CreateRepository<Role>(context);

            // Act
            var result = await repo.GetByIdAsync(roleId);

            // Assert
            result.Should().NotBeNull();
            result.RoleId.Should().Be(roleId);
            result.Name.Should().Be("HR");
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllEntities()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            context.Roles.AddRange(
                new Role { RoleId = Guid.NewGuid(), Name = "RoleA" },
                new Role { RoleId = Guid.NewGuid(), Name = "RoleB" }
            );
            await context.SaveChangesAsync();

            var repo = DbContextHelper.CreateRepository<Role>(context);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesAndPersistsEntity()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var role = new Role { RoleId = Guid.NewGuid(), Name = "InitialName" };
            context.Roles.Add(role);
            await context.SaveChangesAsync();

            var repo = DbContextHelper.CreateRepository<Role>(context);

            // Act
            role.Name = "UpdatedName";
            await repo.UpdateAsync(role);

            // Assert
            var updated = await context.Roles.FindAsync(role.RoleId);
            updated!.Name.Should().Be("UpdatedName");
        }

        [Fact]
        public async Task DeleteAsync_RemovesAndPersistsEntity()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var role = new Role { RoleId = Guid.NewGuid(), Name = "ToDelete" };
            context.Roles.Add(role);
            await context.SaveChangesAsync();

            var repo = DbContextHelper.CreateRepository<Role>(context);

            // Act
            await repo.DeleteAsync(role);

            // Assert
            var deleted = await context.Roles.FindAsync(role.RoleId);
            deleted.Should().BeNull();
        }

        [Fact]
        public void GetUsers_ReturnsQueryableUsers()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            context.Users.Add(new User
            {
                UserId = Guid.NewGuid(),
                Username = "queried_user",
                Password = "pwd"
            });
            context.SaveChanges();

            IGenericRepository<Microsoft.EntityFrameworkCore.DbContext, User> repo = DbContextHelper.CreateRepository<User>(context);

            // Act
            var users = repo.GetUsers();

            // Assert
            users.Should().NotBeNull();
            users.Where(u => u.Username == "queried_user").Count().Should().Be(1);
        }

        [Fact]
        public void GetRoles_ReturnsQueryableRoles()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            context.Roles.Add(new Role
            {
                RoleId = Guid.NewGuid(),
                Name = "SpecialRole"
            });
            context.SaveChanges();

            IGenericRepository<Microsoft.EntityFrameworkCore.DbContext, Role> repo = DbContextHelper.CreateRepository<Role>(context);

            // Act
            var roles = repo.GetRoles();

            // Assert
            roles.Should().NotBeNull();
            roles.Where(r => r.Name == "SpecialRole").Count().Should().Be(1);
        }

        [Fact]
        public async Task UnitOfWork_SaveChangesAsync_PersistsChanges()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var uow = DbContextHelper.CreateUnitOfWork(context);

            context.Roles.Add(new Role { RoleId = Guid.NewGuid(), Name = "UowRole" });

            // Act
            var count = await uow.SaveChangesAsync();

            // Assert
            count.Should().Be(1);
            context.Roles.Count().Should().Be(1);
        }
    }
}
