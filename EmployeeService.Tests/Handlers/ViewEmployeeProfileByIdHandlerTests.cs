using EmployeeService.domains.EmployeeServiceDepartmentService.domains;
using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Business_Handler;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Exceptions;
using FluentAssertions;
using Xunit;

namespace EmployeeService.Tests.Handlers
{
    public class ViewEmployeeProfileByIdHandlerTests
    {
        [Fact]
        public async Task Handle_ValidRequest_ReturnsEmployeeProfileWithAddress()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var empId = Guid.NewGuid();

            context.Employees.Add(new Employees
            {
                EmployeeId = empId,
                EmployeeNumber = 19001,
                FirstName = "Natasha",
                LastName = "Romanoff",
                Email = "natasha@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow.Date.AddYears(-2),
                Status = true
            });
            context.employeeAddresses.Add(new EmployeeAddress
            {
                AddressId = Guid.NewGuid(),
                EmployeeId = empId,
                AddressLine1 = "Safehouse Ave",
                City = "Budapest",
                State = "Pest",
                PostalCode = "1011",
                Country = "Hungary",
                AddressType = "Safehouse"
            });
            await context.SaveChangesAsync();

            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var addressRepo = DbContextHelper.CreateRepository<EmployeeAddress>(context);
            var logger = DbContextHelper.CreateLogger<ViewEmployeeProfileByIdHandler>();

            var handler = new ViewEmployeeProfileByIdHandler(employeeRepo, addressRepo, logger);

            var request = new EmployeeProfileByIdRequestDto
            {
                EmployeeId = empId
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.EmployeeId.Should().Be(empId);
            result.EmployeeNumber.Should().Be(19001);
            result.FullName.Should().Be("Natasha Romanoff");
            result.Email.Should().Be("natasha@example.com");
            result.City.Should().Be("Budapest");
            result.AddressLine1.Should().Be("Safehouse Ave");
        }

        [Fact]
        public async Task Handle_EmployeeNotFound_ThrowsNotFoundException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var addressRepo = DbContextHelper.CreateRepository<EmployeeAddress>(context);
            var logger = DbContextHelper.CreateLogger<ViewEmployeeProfileByIdHandler>();

            var handler = new ViewEmployeeProfileByIdHandler(employeeRepo, addressRepo, logger);

            var nonExistentId = Guid.NewGuid();
            var request = new EmployeeProfileByIdRequestDto
            {
                EmployeeId = nonExistentId
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"*Employee with EmployeeId '{nonExistentId}' not found.*");
        }
    }
}
