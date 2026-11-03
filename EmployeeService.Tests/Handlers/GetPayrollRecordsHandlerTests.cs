using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Business_Handler;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using FluentAssertions;
using Xunit;

namespace EmployeeService.Tests.Handlers
{
    public class GetPayrollRecordsHandlerTests
    {
        [Fact]
        public async Task Handle_ValidRequest_RecordsFound_ReturnsRecords()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var empId = Guid.NewGuid();

            context.Employees.Add(new Employees
            {
                EmployeeId = empId,
                EmployeeNumber = 15001,
                FirstName = "Hal",
                LastName = "Jordan",
                Email = "hal@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            context.PayrollRecords.Add(new PayrollRecords
            {
                PayrollRecordId = Guid.NewGuid(),
                EmployeeId = empId,
                PayrollMonth = 4,
                PayrollYear = 2025,
                BaseSalary = 8000,
                Allowances = 1000,
                Deductions = 500,
                GeneratedDate = DateTime.UtcNow,
                WorkingDays = 22,
                PresentDays = 20,
                LeaveDays = 2,
                HolidayDays = 0,
                WeekendDays = 8,
                PayableDays = 22
            });
            await context.SaveChangesAsync();

            var logger = DbContextHelper.CreateLogger<GetPayrollRecordsHandler>();
            var handler = new GetPayrollRecordsHandler(context, logger);

            var request = new GetPayrollRecordsRequestDto
            {
                EmployeeNumber = 15001
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.PayrollRecords.Should().HaveCount(1);
            result.PayrollRecords[0].BaseSalary.Should().Be(8000);
            result.PayrollRecords[0].EmployeeName.Should().Be("Hal Jordan");
        }

        [Fact]
        public async Task Handle_ValidRequest_NoRecordsFound_ReturnsEmptyRecords()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            context.Employees.Add(new Employees
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeNumber = 15002,
                FirstName = "Victor",
                LastName = "Stone",
                Email = "victor@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            await context.SaveChangesAsync();

            var logger = DbContextHelper.CreateLogger<GetPayrollRecordsHandler>();
            var handler = new GetPayrollRecordsHandler(context, logger);

            var request = new GetPayrollRecordsRequestDto
            {
                EmployeeNumber = 15002
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("No payroll records found for this employee");
            result.PayrollRecords.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_EmployeeNotFound_ReturnsFailure()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var logger = DbContextHelper.CreateLogger<GetPayrollRecordsHandler>();
            var handler = new GetPayrollRecordsHandler(context, logger);

            var request = new GetPayrollRecordsRequestDto
            {
                EmployeeNumber = 99999
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Employee not found");
        }
    }
}
