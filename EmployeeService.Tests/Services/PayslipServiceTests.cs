using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;

namespace EmployeeService.Tests.Services
{
    public class PayslipServiceTests : IDisposable
    {
        private readonly string _tempDirectory;
        private readonly Mock<IWebHostEnvironment> _mockEnv;

        public PayslipServiceTests()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "PayslipServiceTests_" + Guid.NewGuid());
            Directory.CreateDirectory(_tempDirectory);

            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockEnv.Setup(e => e.WebRootPath).Returns(_tempDirectory);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_tempDirectory))
                {
                    Directory.Delete(_tempDirectory, true);
                }
            }
            catch
            {
                // ignore cleanup errors
            }
        }

        [Fact]
        public async Task GeneratePayslipPdfAsync_CreatesValidPdfFile()
        {
            // Arrange
            var logger = DbContextHelper.CreateLogger<PayslipService>();
            var service = new PayslipService(_mockEnv.Object, logger);

            var employee = new Employees
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeNumber = 1005,
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane@example.com",
                Phone = "9876543210",
                HireDate = new DateTime(2023, 5, 1)
            };

            var payroll = new PayrollRecords
            {
                PayrollRecordId = Guid.NewGuid(),
                EmployeeId = employee.EmployeeId,
                BaseSalary = 50000,
                Allowances = 10000,
                Deductions = 5000,
                PayrollMonth = 10,
                PayrollYear = 2026,
                GeneratedDate = DateTime.UtcNow
            };

            // Act
            var relativePath = await service.GeneratePayslipPdfAsync("Jane Doe", 1005, payroll, employee, "Software Engineer");

            // Assert
            relativePath.Should().NotBeNullOrWhiteSpace();
            relativePath.Should().StartWith("/payslips/Payslip_1005_2026_10_");
            relativePath.Should().EndWith(".pdf");

            var fullPath = Path.Combine(_tempDirectory, relativePath.TrimStart('/'));
            File.Exists(fullPath).Should().BeTrue();
            new FileInfo(fullPath).Length.Should().BeGreaterThan(0);
        }
    }
}
