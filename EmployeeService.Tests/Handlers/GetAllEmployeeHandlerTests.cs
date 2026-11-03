using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Business_Handler;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using FluentAssertions;
using Xunit;

namespace EmployeeService.Tests.Handlers
{
    public class GetAllEmployeeHandlerTests
    {
        [Fact]
        public async Task Handle_ActiveEmployeesExist_ReturnsOnlyActiveEmployees()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            context.Employees.Add(new Employees
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeNumber = 11001,
                FirstName = "Bruce",
                LastName = "Wayne",
                Email = "bruce@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            context.Employees.Add(new Employees
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeNumber = 11002,
                FirstName = "Clark",
                LastName = "Kent",
                Email = "clark@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = false // inactive
            });
            await context.SaveChangesAsync();

            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var logger = DbContextHelper.CreateLogger<GetAllEmployeeHandler>();

            var handler = new GetAllEmployeeHandler(employeeRepo, logger);

            var request = new GetAllEmployeeRequestDto();

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Employees.Should().HaveCount(1);
            result.Employees[0].FullName.Should().Be("Bruce Wayne");
            result.Employees[0].Status.Should().Be("Active");
        }

        [Fact]
        public async Task Handle_NoActiveEmployees_ReturnsEmptyList()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            context.Employees.Add(new Employees
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeNumber = 11003,
                FirstName = "Inactive",
                LastName = "User",
                Email = "inactive@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = false
            });
            await context.SaveChangesAsync();

            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var logger = DbContextHelper.CreateLogger<GetAllEmployeeHandler>();

            var handler = new GetAllEmployeeHandler(employeeRepo, logger);

            var request = new GetAllEmployeeRequestDto();

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Employees.Should().BeEmpty();
        }
    }
}
