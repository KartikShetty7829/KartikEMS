using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Business_Handler;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EmployeeService.Tests.Handlers
{
    public class GeneratePayslipHandlerTests
    {
        private readonly Mock<IPayslipService> _mockPayslipService;

        public GeneratePayslipHandlerTests()
        {
            _mockPayslipService = new Mock<IPayslipService>();
        }

        [Fact]
        public async Task Handle_ValidRequest_GeneratesPayslipAndReturnsSuccess()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var empId = Guid.NewGuid();
            var desigId = Guid.NewGuid();

            var employee = new Employees
            {
                EmployeeId = empId,
                EmployeeNumber = 10001,
                FirstName = "Tony",
                LastName = "Stark",
                Email = "tony@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            };
            var designation = new Designation
            {
                DesignationId = desigId,
                Title = "Chief Executive Officer"
            };
            var empDesignation = new EmployeeDesignation
            {
                EmployeeDesignationId = Guid.NewGuid(),
                EmployeeId = empId,
                DesignationId = desigId,
                Designation = designation,
                StartDate = DateTime.UtcNow.Date.AddYears(-1),
                EndDate = DateTime.UtcNow.Date.AddYears(1)
            };
            var payrollRecord = new PayrollRecords
            {
                PayrollRecordId = Guid.NewGuid(),
                EmployeeId = empId,
                PayrollMonth = 5,
                PayrollYear = 2025,
                BaseSalary = 10000,
                GeneratedDate = DateTime.UtcNow,
                WorkingDays = 22,
                PresentDays = 20,
                LeaveDays = 2,
                HolidayDays = 0,
                WeekendDays = 8,
                PayableDays = 22
            };

            context.Employees.Add(employee);
            context.Designations.Add(designation);
            context.EmployeeDesignations.Add(empDesignation);
            context.PayrollRecords.Add(payrollRecord);
            await context.SaveChangesAsync();

            _mockPayslipService
                .Setup(p => p.GeneratePayslipPdfAsync(
                    It.IsAny<string>(),
                    10001,
                    It.IsAny<PayrollRecords>(),
                    It.IsAny<Employees>(),
                    "Chief Executive Officer",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("/payslips/10001_5_2025.pdf");

            var logger = DbContextHelper.CreateLogger<GeneratePayslipHandler>();
            var handler = new GeneratePayslipHandler(context, _mockPayslipService.Object, logger);

            var request = new GeneratePayslipRequestDto
            {
                EmployeeNumber = 10001,
                PayrollMonth = 5,
                PayrollYear = 2025
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.FilePath.Should().Be("/payslips/10001_5_2025.pdf");

            var payslip = await context.Payslips.FirstOrDefaultAsync(p => p.EmployeeId == empId);
            payslip.Should().NotBeNull();
            payslip!.PdfUrl.Should().Be("/payslips/10001_5_2025.pdf");
        }

        [Fact]
        public async Task Handle_InvalidEmployeeNumber_ReturnsFailure()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var logger = DbContextHelper.CreateLogger<GeneratePayslipHandler>();
            var handler = new GeneratePayslipHandler(context, _mockPayslipService.Object, logger);

            var request = new GeneratePayslipRequestDto
            {
                EmployeeNumber = 0,
                PayrollMonth = 5,
                PayrollYear = 2025
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Invalid employee number");
        }

        [Fact]
        public async Task Handle_InvalidMonth_ReturnsFailure()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var logger = DbContextHelper.CreateLogger<GeneratePayslipHandler>();
            var handler = new GeneratePayslipHandler(context, _mockPayslipService.Object, logger);

            var request = new GeneratePayslipRequestDto
            {
                EmployeeNumber = 10001,
                PayrollMonth = 13,
                PayrollYear = 2025
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Invalid payroll month");
        }

        [Fact]
        public async Task Handle_EmployeeNotFound_ReturnsFailure()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var logger = DbContextHelper.CreateLogger<GeneratePayslipHandler>();
            var handler = new GeneratePayslipHandler(context, _mockPayslipService.Object, logger);

            var request = new GeneratePayslipRequestDto
            {
                EmployeeNumber = 99999,
                PayrollMonth = 5,
                PayrollYear = 2025
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("not found");
        }

        [Fact]
        public async Task Handle_PayrollRecordNotFound_ReturnsFailure()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            context.Employees.Add(new Employees
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeNumber = 10002,
                FirstName = "Steve",
                LastName = "Rogers",
                Email = "steve@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            await context.SaveChangesAsync();

            var logger = DbContextHelper.CreateLogger<GeneratePayslipHandler>();
            var handler = new GeneratePayslipHandler(context, _mockPayslipService.Object, logger);

            var request = new GeneratePayslipRequestDto
            {
                EmployeeNumber = 10002,
                PayrollMonth = 5,
                PayrollYear = 2025
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Payroll record not found");
        }
    }
}
