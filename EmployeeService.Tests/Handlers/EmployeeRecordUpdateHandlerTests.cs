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
    public class EmployeeRecordUpdateHandlerTests
    {
        [Fact]
        public async Task Handle_ValidRequest_UpdatesFieldsSuccessfully()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var empId = Guid.NewGuid();
            context.Employees.Add(new Employees
            {
                EmployeeId = empId,
                EmployeeNumber = 9001,
                FirstName = "Sam",
                LastName = "Winchester",
                Email = "sam@example.com",
                Phone = "1112223333",
                HireDate = DateTime.UtcNow.Date.AddYears(-2),
                Status = true
            });
            await context.SaveChangesAsync();

            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<EmployeeRecordUpdateHandler>();

            var handler = new EmployeeRecordUpdateHandler(employeeRepo, logger, unitOfWork, context);

            var newHireDate = DateTime.UtcNow.Date.AddYears(-1);
            var request = new EmployeeRecordUpdateRequestDto
            {
                EmployeeNumber = 9001,
                FirstName = "Samuel",
                LastName = "Campbell",
                Email = "samuel@example.com",
                phonenumber = "9998887777",
                HireDate = newHireDate
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be(CommonMessages.EmployeeUpdate);

            var updatedEmp = await context.Employees.FirstOrDefaultAsync(e => e.EmployeeNumber == 9001);
            updatedEmp!.FirstName.Should().Be("Samuel");
            updatedEmp.LastName.Should().Be("Campbell");
            updatedEmp.Email.Should().Be("samuel@example.com");
            updatedEmp.Phone.Should().Be("9998887777");
            updatedEmp.HireDate.Should().Be(newHireDate);
        }

        [Fact]
        public async Task Handle_EmployeeNotFound_ThrowsNotFoundException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<EmployeeRecordUpdateHandler>();

            var handler = new EmployeeRecordUpdateHandler(employeeRepo, logger, unitOfWork, context);

            var request = new EmployeeRecordUpdateRequestDto
            {
                EmployeeNumber = 9999,
                FirstName = "Ghost"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*Employee with Id '9999' not found.*");
        }
    }
}
