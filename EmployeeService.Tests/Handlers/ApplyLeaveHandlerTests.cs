using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Business_Handler;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Exceptions;
using EmployeeServiceDepartmentService.interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EmployeeService.Tests.Handlers
{
    public class ApplyLeaveHandlerTests
    {
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IMessageService> _mockMessageService;
        private readonly Mock<IKafkaProducerService> _mockKafkaService;

        public ApplyLeaveHandlerTests()
        {
            _mockEmailService = new Mock<IEmailService>();
            _mockMessageService = new Mock<IMessageService>();
            _mockKafkaService = new Mock<IKafkaProducerService>();
        }

        [Fact]
        public async Task Handle_ValidRequest_CreatesLeaveAndProducesKafkaEvent()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var empId = Guid.NewGuid();
            context.Employees.Add(new Employees
            {
                EmployeeId = empId,
                EmployeeNumber = 2001,
                FirstName = "David",
                LastName = "Miller",
                Email = "david@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            await context.SaveChangesAsync();

            var leaveRepo = DbContextHelper.CreateRepository<LeaveRequests>(context);
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<ApplyLeaveHandler>();

            var handler = new ApplyLeaveHandler(
                leaveRepo,
                employeeRepo,
                unitOfWork,
                logger,
                _mockEmailService.Object,
                _mockMessageService.Object,
                _mockKafkaService.Object);

            var request = new ApplyLeaveRequestDto
            {
                EmployeeNumber = 2001,
                Type = "Casual",
                StartDate = DateTime.UtcNow.Date.AddDays(5),
                EndDate = DateTime.UtcNow.Date.AddDays(7)
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Email.Should().Be("david@example.com");

            var savedLeave = await context.leaveRequest.FirstOrDefaultAsync(l => l.EmployeeId == empId);
            savedLeave.Should().NotBeNull();
            savedLeave!.Status.Should().Be("Pending");
            savedLeave.Type.Should().Be("Casual");

            _mockKafkaService.Verify(
                k => k.ProduceAsync("leave-events", It.Is<string>(msg => msg.Contains("LeaveApplied"))),
                Times.Once);

            _mockEmailService.Verify(
                e => e.SendEmailAsync("david@example.com", It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EndDateBeforeStartDate_ThrowsBadRequestException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var leaveRepo = DbContextHelper.CreateRepository<LeaveRequests>(context);
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<ApplyLeaveHandler>();

            var handler = new ApplyLeaveHandler(
                leaveRepo, employeeRepo, unitOfWork, logger,
                _mockEmailService.Object, _mockMessageService.Object, _mockKafkaService.Object);

            var request = new ApplyLeaveRequestDto
            {
                EmployeeNumber = 2001,
                StartDate = DateTime.UtcNow.Date.AddDays(10),
                EndDate = DateTime.UtcNow.Date.AddDays(5)
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("*End date cannot be earlier than start date*");
        }

        [Fact]
        public async Task Handle_EmployeeNotFound_ThrowsNotFoundException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var leaveRepo = DbContextHelper.CreateRepository<LeaveRequests>(context);
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<ApplyLeaveHandler>();

            var handler = new ApplyLeaveHandler(
                leaveRepo, employeeRepo, unitOfWork, logger,
                _mockEmailService.Object, _mockMessageService.Object, _mockKafkaService.Object);

            var request = new ApplyLeaveRequestDto
            {
                EmployeeNumber = 9999,
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(1)
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*Employee not found*");
        }

        [Fact]
        public async Task Handle_OverlappingLeaveRequest_ThrowsBadRequestException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var empId = Guid.NewGuid();
            context.Employees.Add(new Employees
            {
                EmployeeId = empId,
                EmployeeNumber = 2002,
                FirstName = "Eva",
                LastName = "Green",
                Email = "eva@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            context.leaveRequest.Add(new LeaveRequests
            {
                LeaveId = Guid.NewGuid(),
                EmployeeId = empId,
                Type = "Annual",
                StartDate = DateTime.UtcNow.Date.AddDays(5),
                EndDate = DateTime.UtcNow.Date.AddDays(10),
                Status = "Approved"
            });
            await context.SaveChangesAsync();

            var leaveRepo = DbContextHelper.CreateRepository<LeaveRequests>(context);
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<ApplyLeaveHandler>();

            var handler = new ApplyLeaveHandler(
                leaveRepo, employeeRepo, unitOfWork, logger,
                _mockEmailService.Object, _mockMessageService.Object, _mockKafkaService.Object);

            var request = new ApplyLeaveRequestDto
            {
                EmployeeNumber = 2002,
                StartDate = DateTime.UtcNow.Date.AddDays(8),
                EndDate = DateTime.UtcNow.Date.AddDays(12)
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("*already exists for the given date range*");
        }
    }
}
