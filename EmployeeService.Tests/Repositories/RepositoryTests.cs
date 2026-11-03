using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Entity;
using FluentAssertions;
using Xunit;

namespace EmployeeService.Tests.Repositories
{
    public class RepositoryTests
    {
        [Fact]
        public async Task ReadOnlyRepository_QueryAndGetById_ReturnsEntity()
        {
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var empId = Guid.NewGuid();
            var emp = new Employees
            {
                EmployeeId = empId,
                EmployeeNumber = 101,
                FirstName = "ReadOnly",
                LastName = "User",
                Email = "readonly@example.com",
                Phone = "12345",
                HireDate = DateTime.UtcNow
            };
            context.Employees.Add(emp);
            await context.SaveChangesAsync();

            var repo = DbContextHelper.CreateReadOnlyRepository<Employees>(context);

            var query = repo.Query();
            query.Should().NotBeNull();
            query.Count().Should().Be(1);

            var entity = await repo.GetByIdAsync(empId);
            entity.Should().NotBeNull();
            entity.FirstName.Should().Be("ReadOnly");

            var all = await repo.GetAllAsync();
            all.Should().HaveCount(1);

            var found = await repo.FindAsync(e => e.EmployeeNumber == 101);
            found.Should().HaveCount(1);
        }

        [Fact]
        public async Task GenericRepository_AddUpdateDelete_PerformsOperations()
        {
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var repo = DbContextHelper.CreateRepository<Employees>(context);

            var emp = new Employees
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeNumber = 102,
                FirstName = "Generic",
                LastName = "User",
                Email = "generic@example.com",
                Phone = "54321",
                HireDate = DateTime.UtcNow
            };

            // Add
            await repo.AddAsync(emp);
            await context.SaveChangesAsync();
            context.Employees.Count().Should().Be(1);

            // Update
            emp.FirstName = "UpdatedName";
            await repo.UpdateAsync(emp);
            await context.SaveChangesAsync();
            var updated = await context.Employees.FindAsync(emp.EmployeeId);
            updated!.FirstName.Should().Be("UpdatedName");

            // Delete
            await repo.DeleteAsync(emp);
            await context.SaveChangesAsync();
            context.Employees.Count().Should().Be(0);
        }

        [Fact]
        public async Task GenericRepository_NullEntity_ThrowsArgumentNullException()
        {
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var repo = DbContextHelper.CreateRepository<Employees>(context);

            Func<Task> actAdd = async () => await repo.AddAsync(null!);
            await actAdd.Should().ThrowAsync<ArgumentNullException>();

            Func<Task> actUpdate = async () => await repo.UpdateAsync(null!);
            await actUpdate.Should().ThrowAsync<ArgumentNullException>();

            Func<Task> actDelete = async () => await repo.DeleteAsync(null!);
            await actDelete.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task UnitOfWork_SaveChangesAsync_SavesContextChanges()
        {
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var uow = DbContextHelper.CreateUnitOfWork(context);

            context.Employees.Add(new Employees
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeNumber = 103,
                FirstName = "Unit",
                LastName = "Work",
                Email = "uow@example.com",
                Phone = "999",
                HireDate = DateTime.UtcNow
            });

            var count = await uow.SaveChangesAsync();
            count.Should().Be(1);
        }
    }
}
