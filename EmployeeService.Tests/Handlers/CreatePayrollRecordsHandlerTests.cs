using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Business_Handler;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using FluentAssertions;
using Xunit;

namespace EmployeeService.Tests.Handlers
{
    public class CreatePayrollRecordsHandlerTests
    {
        [Fact]
        public async Task Handle_ValidRequest_GeneratesPayrollSuccessfully()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var empId = Guid.NewGuid();
            context.Employees.Add(new Employees
            {
                EmployeeId = empId,
                EmployeeNumber = 5001,
                FirstName = "Maria",
                LastName = "Garcia",
                Email = "maria@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            context.PayrollRecords.Add(new PayrollRecords
            {
                PayrollRecordId = Guid.NewGuid(),
                EmployeeId = empId,
                GeneratedDate = DateTime.UtcNow.Date,
                BaseSalary = 5000,
                PayrollMonth = DateTime.UtcNow.Month,
                PayrollYear = DateTime.UtcNow.Year,
                WorkingDays = 22,
                PresentDays = 20,
                LeaveDays = 2,
                HolidayDays = 0,
                WeekendDays = 8,
                PayableDays = 22
            });
            await context.SaveChangesAsync();

            var payrollRepo = DbContextHelper.CreateRepository<PayrollRecords>(context);
            var logger = DbContextHelper.CreateLogger<CreatePayrollRecordsHandler>();

            var handler = new CreatePayrollRecordsHandler(context, payrollRepo, logger);

            var request = new CreatePayrollRecordsRequestDto
            {
                EmployeeNumber = 5001,
                StartDate = DateTime.UtcNow.Date.AddDays(-1),
                EndDate = DateTime.UtcNow.Date.AddDays(1)
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.RecordsGenerated.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EmployeeNotFound_ReturnsFailure()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var payrollRepo = DbContextHelper.CreateRepository<PayrollRecords>(context);
            var logger = DbContextHelper.CreateLogger<CreatePayrollRecordsHandler>();

            var handler = new CreatePayrollRecordsHandler(context, payrollRepo, logger);

            var request = new CreatePayrollRecordsRequestDto
            {
                EmployeeNumber = 9999,
                StartDate = DateTime.UtcNow.Date.AddDays(-1),
                EndDate = DateTime.UtcNow.Date.AddDays(1)
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Employee not found");
        }

        [Fact]
        public async Task Handle_StartDateAfterEndDate_ReturnsFailure()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            context.Employees.Add(new Employees
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeNumber = 5002,
                FirstName = "Nina",
                LastName = "Simon",
                Email = "nina@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            await context.SaveChangesAsync();

            var payrollRepo = DbContextHelper.CreateRepository<PayrollRecords>(context);
            var logger = DbContextHelper.CreateLogger<CreatePayrollRecordsHandler>();

            var handler = new CreatePayrollRecordsHandler(context, payrollRepo, logger);

            var request = new CreatePayrollRecordsRequestDto
            {
                EmployeeNumber = 5002,
                StartDate = DateTime.UtcNow.Date.AddDays(5),
                EndDate = DateTime.UtcNow.Date.AddDays(1)
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("StartDate must be before or equal to EndDate");
        }
    }
}
