using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Business_Handler;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Exceptions;
using FluentAssertions;
using Xunit;

namespace EmployeeService.Tests.Handlers
{
    public class ViewEmployeeProfileHandlerTests
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
                EmployeeNumber = 20001,
                FirstName = "Scott",
                LastName = "Lang",
                Email = "scott@example.com",
                Phone = "5551112222",
                HireDate = DateTime.UtcNow.Date.AddYears(-1),
                Status = true
            });
            context.employeeAddresses.Add(new EmployeeAddress
            {
                AddressId = Guid.NewGuid(),
                EmployeeId = empId,
                AddressLine1 = "Van Dyke St",
                City = "San Francisco",
                State = "CA",
                PostalCode = "94101",
                Country = "USA",
                AddressType = "Home"
            });
            await context.SaveChangesAsync();

            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var addressRepo = DbContextHelper.CreateRepository<EmployeeAddress>(context);
            var logger = DbContextHelper.CreateLogger<ViewEmployeeProfileHandler>();

            var handler = new ViewEmployeeProfileHandler(employeeRepo, addressRepo, logger);

            var request = new EmployeeProfileRequestDto
            {
                EmployeeNumber = 20001
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.EmployeeId.Should().Be(empId);
            result.EmployeeNumber.Should().Be(20001);
            result.FullName.Should().Be("Scott Lang");
            result.Email.Should().Be("scott@example.com");
            result.City.Should().Be("San Francisco");
            result.AddressLine1.Should().Be("Van Dyke St");
        }

        [Fact]
        public async Task Handle_EmployeeNotFound_ThrowsNotFoundException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var addressRepo = DbContextHelper.CreateRepository<EmployeeAddress>(context);
            var logger = DbContextHelper.CreateLogger<ViewEmployeeProfileHandler>();

            var handler = new ViewEmployeeProfileHandler(employeeRepo, addressRepo, logger);

            var request = new EmployeeProfileRequestDto
            {
                EmployeeNumber = 99999
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*Employee with EmployeeNumber '99999' not found.*");
        }
    }
}
