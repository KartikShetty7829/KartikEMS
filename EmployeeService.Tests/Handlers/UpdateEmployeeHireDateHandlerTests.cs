using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Business_Handler;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EmployeeService.Tests.Handlers
{
    public class UpdateEmployeeHireDateHandlerTests
    {
        [Fact]
        public async Task Handle_ValidRequest_UpdatesHireDateSuccessfully()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            context.Employees.Add(new Employees
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeNumber = 16001,
                FirstName = "Wanda",
                LastName = "Maximoff",
                Email = "wanda@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow.Date.AddYears(-3),
                Status = true
            });
            await context.SaveChangesAsync();

            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<UpdateEmployeeHireDateHandler>();

            var handler = new UpdateEmployeeHireDateHandler(employeeRepo, logger, unitOfWork);

            var newHireDate = DateTime.UtcNow.Date.AddYears(-1);
            var request = new EmployeeUpdateRequestDto
            {
                EmployeeNumber = 16001,
                HireDate = newHireDate
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Contain("Wanda Maximoff");

            var employee = await context.Employees.FirstOrDefaultAsync(e => e.EmployeeNumber == 16001);
            employee!.HireDate.Should().Be(newHireDate);
        }

        [Fact]
        public async Task Handle_EmployeeNotFound_ThrowsNotFoundException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<UpdateEmployeeHireDateHandler>();

            var handler = new UpdateEmployeeHireDateHandler(employeeRepo, logger, unitOfWork);

            var request = new EmployeeUpdateRequestDto
            {
                EmployeeNumber = 99999,
                HireDate = DateTime.UtcNow.Date
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*Employee with Id '99999' not found.*");
        }
    }
}
