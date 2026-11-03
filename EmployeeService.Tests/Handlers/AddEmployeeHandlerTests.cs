using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Business_Handler;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Exceptions;
using EmployeeServiceDepartmentService.interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EmployeeService.Tests.Handlers
{
    public class AddEmployeeHandlerTests
    {
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IMessageService> _mockMessageService;

        public AddEmployeeHandlerTests()
        {
            _mockEmailService = new Mock<IEmailService>();
            _mockMessageService = new Mock<IMessageService>();
        }

        [Fact]
        public async Task Handle_ValidRequest_CreatesEmployeeAndReturnsSuccess()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<AddEmployeeHandler>();

            var handler = new AddEmployeeHandler(
                employeeRepo,
                logger,
                unitOfWork,
                _mockEmailService.Object,
                _mockMessageService.Object);

            var request = new EmployeeRequestDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow.Date,
                Status = true
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.EmployeeId.Should().NotBeEmpty();

            var savedEmployee = context.Employees.FirstOrDefault(e => e.Email == "john.doe@example.com");
            savedEmployee.Should().NotBeNull();
            savedEmployee!.FirstName.Should().Be("John");
            savedEmployee.LastName.Should().Be("Doe");

            _mockEmailService.Verify(
                e => e.SendEmailAsync(request.Email, It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);

            _mockMessageService.Verify(
                m => m.SendSmsAsync(request.Phone, It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingEmail_ThrowsBadRequestException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            context.Employees.Add(new Employees
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeNumber = 101,
                FirstName = "Existing",
                LastName = "User",
                Email = "duplicate@example.com",
                Phone = "9999999999",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            await context.SaveChangesAsync();

            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<AddEmployeeHandler>();

            var handler = new AddEmployeeHandler(
                employeeRepo,
                logger,
                unitOfWork,
                _mockEmailService.Object,
                _mockMessageService.Object);

            var request = new EmployeeRequestDto
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "duplicate@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task Handle_EmailServiceFails_StillCreatesEmployeeSuccessfully()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<AddEmployeeHandler>();

            _mockEmailService
                .Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("SMTP error"));

            var handler = new AddEmployeeHandler(
                employeeRepo,
                logger,
                unitOfWork,
                _mockEmailService.Object,
                _mockMessageService.Object);

            var request = new EmployeeRequestDto
            {
                FirstName = "Mark",
                LastName = "Spencer",
                Email = "mark@example.com",
                Phone = "5555555555",
                HireDate = DateTime.UtcNow,
                Status = true
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            context.Employees.Any(e => e.Email == "mark@example.com").Should().BeTrue();
        }
    }
}
