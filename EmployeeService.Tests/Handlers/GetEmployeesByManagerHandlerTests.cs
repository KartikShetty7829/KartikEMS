using EmployeeService.Business_Handler;
using EmployeeService.domains;
using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Entity;
using FluentAssertions;
using Xunit;

namespace EmployeeService.Tests.Handlers
{
    public class GetEmployeesByManagerHandlerTests
    {
        [Fact]
        public async Task Handle_ManagerHasDirectReports_ReturnsReportingEmployees()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var managerId = Guid.NewGuid();

            context.Employees.Add(new Employees
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeNumber = 13001,
                FirstName = "Barry",
                LastName = "Allen",
                Email = "barry@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                ManagerId = managerId,
                Status = true
            });
            context.Employees.Add(new Employees
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeNumber = 13002,
                FirstName = "Cisco",
                LastName = "Ramon",
                Email = "cisco@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                ManagerId = managerId,
                Status = true
            });
            context.Employees.Add(new Employees
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeNumber = 13003,
                FirstName = "Other",
                LastName = "Employee",
                Email = "other@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                ManagerId = Guid.NewGuid(),
                Status = true
            });
            await context.SaveChangesAsync();

            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var handler = new GetEmployeesByManagerHandler(employeeRepo);

            var request = new GetEmployeesByManagerRequestDto
            {
                ManagerId = managerId
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(e => e.FullName == "Barry Allen");
            result.Should().Contain(e => e.FullName == "Cisco Ramon");
        }

        [Fact]
        public async Task Handle_ManagerHasNoReports_ReturnsEmptyList()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var handler = new GetEmployeesByManagerHandler(employeeRepo);

            var request = new GetEmployeesByManagerRequestDto
            {
                ManagerId = Guid.NewGuid()
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }
    }
}
