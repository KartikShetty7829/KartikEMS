using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Business_Handler;
using EmployeeServiceDepartmentService.Common;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EmployeeService.Tests.Handlers
{
    public class EmployeeRecordDeleteHandlerTests
    {
        [Fact]
        public async Task Handle_ValidRequest_Inactive_MarksEmployeeInactive()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var empId = Guid.NewGuid();
            context.Employees.Add(new Employees
            {
                EmployeeId = empId,
                EmployeeNumber = 8001,
                FirstName = "Quinn",
                LastName = "Fabray",
                Email = "quinn@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            await context.SaveChangesAsync();

            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<EmployeeRecordDeleteHandler>();

            var handler = new EmployeeRecordDeleteHandler(employeeRepo, logger, unitOfWork, context);

            var request = new EmployeeRecordDeleteRequestDto
            {
                EmployeeNumber = 8001,
                Status = EmployeeStatus.Inactive
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be(CommonMessages.EmployeeMarkedInactive);

            var updatedEmp = await context.Employees.FirstOrDefaultAsync(e => e.EmployeeNumber == 8001);
            updatedEmp!.Status.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ValidRequest_Active_MarksEmployeeActive()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var empId = Guid.NewGuid();
            context.Employees.Add(new Employees
            {
                EmployeeId = empId,
                EmployeeNumber = 8002,
                FirstName = "Rachel",
                LastName = "Berry",
                Email = "rachel@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = false
            });
            await context.SaveChangesAsync();

            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<EmployeeRecordDeleteHandler>();

            var handler = new EmployeeRecordDeleteHandler(employeeRepo, logger, unitOfWork, context);

            var request = new EmployeeRecordDeleteRequestDto
            {
                EmployeeNumber = 8002,
                Status = EmployeeStatus.Active
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be(CommonMessages.EmployeeMarkedActive);

            var updatedEmp = await context.Employees.FirstOrDefaultAsync(e => e.EmployeeNumber == 8002);
            updatedEmp!.Status.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EmployeeNotFound_ThrowsNotFoundException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<EmployeeRecordDeleteHandler>();

            var handler = new EmployeeRecordDeleteHandler(employeeRepo, logger, unitOfWork, context);

            var request = new EmployeeRecordDeleteRequestDto
            {
                EmployeeNumber = 9999,
                Status = EmployeeStatus.Inactive
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*Employee with Id '9999' not found.*");
        }
    }
}
