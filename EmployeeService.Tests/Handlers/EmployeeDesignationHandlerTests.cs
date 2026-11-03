using EmployeeService.Business_Handler;
using EmployeeService.domains;
using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Exceptions;
using FluentAssertions;
using Xunit;

namespace EmployeeService.Tests.Handlers
{
    public class EmployeeDesignationHandlerTests
    {
        [Fact]
        public async Task Handle_ValidRequest_ReturnsEmployeeDesignation()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var empId = Guid.NewGuid();
            var desigId = Guid.NewGuid();

            var employee = new Employees
            {
                EmployeeId = empId,
                EmployeeNumber = 6001,
                FirstName = "Oliver",
                LastName = "Queen",
                Email = "oliver@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            };
            var designation = new Designation
            {
                DesignationId = desigId,
                Title = "Technical Lead",
                Description = "Lead tech role"
            };
            var employeeDesignation = new EmployeeDesignation
            {
                EmployeeDesignationId = Guid.NewGuid(),
                EmployeeId = empId,
                Employee = employee,
                DesignationId = desigId,
                Designation = designation,
                StartDate = DateTime.UtcNow.Date.AddYears(-1),
                EndDate = DateTime.UtcNow.Date.AddYears(1)
            };

            context.Employees.Add(employee);
            context.Designations.Add(designation);
            context.EmployeeDesignations.Add(employeeDesignation);
            await context.SaveChangesAsync();

            var employeeDesignationRepo = DbContextHelper.CreateRepository<EmployeeDesignation>(context);
            var logger = DbContextHelper.CreateLogger<EmployeeDesignationHandler>();

            var handler = new EmployeeDesignationHandler(employeeDesignationRepo, logger);

            var request = new EmployeeDesignationRequestDto
            {
                EmployeeNumber = 6001
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.EmployeeNumber.Should().Be(6001);
            result.DesignationName.Should().Be("Technical Lead");
        }

        [Fact]
        public async Task Handle_DesignationNotFound_ThrowsNotFoundException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var employeeDesignationRepo = DbContextHelper.CreateRepository<EmployeeDesignation>(context);
            var logger = DbContextHelper.CreateLogger<EmployeeDesignationHandler>();

            var handler = new EmployeeDesignationHandler(employeeDesignationRepo, logger);

            var request = new EmployeeDesignationRequestDto
            {
                EmployeeNumber = 9999
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*Designation not found for employee number 9999*");
        }
    }
}
