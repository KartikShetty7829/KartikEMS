using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Business_Handler;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using FluentAssertions;
using Xunit;

namespace EmployeeService.Tests.Handlers
{
    public class GetEmployeesWithPendingLeavesHandlerTests
    {
        [Fact]
        public async Task Handle_ManagerHasPendingLeaves_ReturnsPendingLeavesList()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var managerId = Guid.NewGuid();
            var empId = Guid.NewGuid();

            context.Employees.Add(new Employees
            {
                EmployeeId = managerId,
                EmployeeNumber = 14001,
                FirstName = "Arthur",
                LastName = "Curry",
                Email = "arthur@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            context.Employees.Add(new Employees
            {
                EmployeeId = empId,
                EmployeeNumber = 14002,
                FirstName = "Mera",
                LastName = "Ocean",
                Email = "mera@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                ManagerId = managerId,
                Status = true
            });
            context.leaveRequest.Add(new LeaveRequests
            {
                LeaveId = Guid.NewGuid(),
                EmployeeId = empId,
                Type = "Vacation",
                StartDate = DateTime.UtcNow.Date.AddDays(3),
                EndDate = DateTime.UtcNow.Date.AddDays(7),
                Status = "Pending"
            });
            await context.SaveChangesAsync();

            var leaveRepo = DbContextHelper.CreateRepository<LeaveRequests>(context);
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var logger = DbContextHelper.CreateLogger<GetEmployeesWithPendingLeavesHandler>();

            var handler = new GetEmployeesWithPendingLeavesHandler(leaveRepo, employeeRepo, logger);

            var request = new GetEmployeesWithPendingLeavesRequestDto
            {
                EmployeeNumber = 14001
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.PendingLeaves.Should().HaveCount(1);
            result.PendingLeaves[0].EmployeeName.Should().Be("Mera Ocean");
            result.PendingLeaves[0].Status.Should().Be("Pending");
        }

        [Fact]
        public async Task Handle_ManagerNotFound_ReturnsEmptyList()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var leaveRepo = DbContextHelper.CreateRepository<LeaveRequests>(context);
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var logger = DbContextHelper.CreateLogger<GetEmployeesWithPendingLeavesHandler>();

            var handler = new GetEmployeesWithPendingLeavesHandler(leaveRepo, employeeRepo, logger);

            var request = new GetEmployeesWithPendingLeavesRequestDto
            {
                EmployeeNumber = 99999
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.PendingLeaves.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ManagerHasNoTeamMembers_ReturnsEmptyList()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            context.Employees.Add(new Employees
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeNumber = 14003,
                FirstName = "Lone",
                LastName = "Wolf",
                Email = "lone@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            await context.SaveChangesAsync();

            var leaveRepo = DbContextHelper.CreateRepository<LeaveRequests>(context);
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var logger = DbContextHelper.CreateLogger<GetEmployeesWithPendingLeavesHandler>();

            var handler = new GetEmployeesWithPendingLeavesHandler(leaveRepo, employeeRepo, logger);

            var request = new GetEmployeesWithPendingLeavesRequestDto
            {
                EmployeeNumber = 14003
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.PendingLeaves.Should().BeEmpty();
        }
    }
}
