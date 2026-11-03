using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Business_Handler;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Exceptions;
using FluentAssertions;
using Xunit;

namespace EmployeeService.Tests.Handlers
{
    public class GetEmployeeLeavesHandlerTests
    {
        [Fact]
        public async Task Handle_ValidRequest_ReturnsEmployeeLeaves()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var empId = Guid.NewGuid();
            context.Employees.Add(new Employees
            {
                EmployeeId = empId,
                EmployeeNumber = 12001,
                FirstName = "Diana",
                LastName = "Prince",
                Email = "diana@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            context.leaveRequest.Add(new LeaveRequests
            {
                LeaveId = Guid.NewGuid(),
                EmployeeId = empId,
                Type = "Sick",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(2),
                Status = "Approved"
            });
            await context.SaveChangesAsync();

            var leaveRepo = DbContextHelper.CreateRepository<LeaveRequests>(context);
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var logger = DbContextHelper.CreateLogger<GetEmployeeLeavesHandler>();

            var handler = new GetEmployeeLeavesHandler(leaveRepo, employeeRepo, logger);

            var request = new GetEmployeeLeavesRequestDto
            {
                EmployeeNumber = 12001
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Leaves.Should().HaveCount(1);
            result.Leaves[0].Type.Should().Be("Sick");
            result.Leaves[0].Status.Should().Be("Approved");
        }

        [Fact]
        public async Task Handle_EmployeeNotFound_ThrowsNotFoundException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var leaveRepo = DbContextHelper.CreateRepository<LeaveRequests>(context);
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var logger = DbContextHelper.CreateLogger<GetEmployeeLeavesHandler>();

            var handler = new GetEmployeeLeavesHandler(leaveRepo, employeeRepo, logger);

            var request = new GetEmployeeLeavesRequestDto
            {
                EmployeeNumber = 99999
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*Employee not found.*");
        }
    }
}
