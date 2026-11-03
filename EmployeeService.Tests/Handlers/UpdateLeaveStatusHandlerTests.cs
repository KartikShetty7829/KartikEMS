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
    public class UpdateLeaveStatusHandlerTests
    {
        private readonly Mock<IKafkaProducerService> _mockKafkaProducer;

        public UpdateLeaveStatusHandlerTests()
        {
            _mockKafkaProducer = new Mock<IKafkaProducerService>();
        }

        [Fact]
        public async Task Handle_ValidRequest_Approved_UpdatesStatusAndProducesKafkaEvent()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var adminId = Guid.NewGuid();
            var leaveId = Guid.NewGuid();
            var empId = Guid.NewGuid();

            context.Employees.Add(new Employees
            {
                EmployeeId = adminId,
                EmployeeNumber = 18001,
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            context.leaveRequest.Add(new LeaveRequests
            {
                LeaveId = leaveId,
                EmployeeId = empId,
                Type = "Annual",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(3),
                Status = "Pending"
            });
            await context.SaveChangesAsync();

            var leaveRepo = DbContextHelper.CreateRepository<LeaveRequests>(context);
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<UpdateLeaveStatusHandler>();

            var handler = new UpdateLeaveStatusHandler(
                leaveRepo, employeeRepo, unitOfWork, logger, _mockKafkaProducer.Object);

            var request = new UpdateLeaveStatusRequestDto
            {
                LeaveId = leaveId,
                AdminEmployeeNumber = 18001,
                Action = "Approved"
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Leave request Approved successfully");

            var updatedLeave = await context.leaveRequest.FirstOrDefaultAsync(l => l.LeaveId == leaveId);
            updatedLeave!.Status.Should().Be("Approved");
            updatedLeave.ApprovedBy.Should().Be(adminId);

            _mockKafkaProducer.Verify(
                k => k.ProduceAsync("leave-events", It.Is<string>(msg => msg.Contains("LeaveApproved"))),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidRequest_Rejected_UpdatesStatusAndProducesKafkaEvent()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var adminId = Guid.NewGuid();
            var leaveId = Guid.NewGuid();

            context.Employees.Add(new Employees
            {
                EmployeeId = adminId,
                EmployeeNumber = 18002,
                FirstName = "Admin2",
                LastName = "User2",
                Email = "admin2@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            context.leaveRequest.Add(new LeaveRequests
            {
                LeaveId = leaveId,
                EmployeeId = Guid.NewGuid(),
                Type = "Casual",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(1),
                Status = "Pending"
            });
            await context.SaveChangesAsync();

            var leaveRepo = DbContextHelper.CreateRepository<LeaveRequests>(context);
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<UpdateLeaveStatusHandler>();

            var handler = new UpdateLeaveStatusHandler(
                leaveRepo, employeeRepo, unitOfWork, logger, _mockKafkaProducer.Object);

            var request = new UpdateLeaveStatusRequestDto
            {
                LeaveId = leaveId,
                AdminEmployeeNumber = 18002,
                Action = "Rejected"
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Leave request Rejected successfully");

            var updatedLeave = await context.leaveRequest.FirstOrDefaultAsync(l => l.LeaveId == leaveId);
            updatedLeave!.Status.Should().Be("Rejected");

            _mockKafkaProducer.Verify(
                k => k.ProduceAsync("leave-events", It.Is<string>(msg => msg.Contains("LeaveRejected"))),
                Times.Once);
        }

        [Fact]
        public async Task Handle_LeaveNotFound_ThrowsNotFoundException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var leaveRepo = DbContextHelper.CreateRepository<LeaveRequests>(context);
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<UpdateLeaveStatusHandler>();

            var handler = new UpdateLeaveStatusHandler(
                leaveRepo, employeeRepo, unitOfWork, logger, _mockKafkaProducer.Object);

            var request = new UpdateLeaveStatusRequestDto
            {
                LeaveId = Guid.NewGuid(),
                AdminEmployeeNumber = 18001,
                Action = "Approved"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*Leave request with ID*not found*");
        }

        [Fact]
        public async Task Handle_AdminEmployeeNotFound_ThrowsNotFoundException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var leaveId = Guid.NewGuid();

            context.leaveRequest.Add(new LeaveRequests
            {
                LeaveId = leaveId,
                EmployeeId = Guid.NewGuid(),
                Type = "Medical",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(1),
                Status = "Pending"
            });
            await context.SaveChangesAsync();

            var leaveRepo = DbContextHelper.CreateRepository<LeaveRequests>(context);
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<UpdateLeaveStatusHandler>();

            var handler = new UpdateLeaveStatusHandler(
                leaveRepo, employeeRepo, unitOfWork, logger, _mockKafkaProducer.Object);

            var request = new UpdateLeaveStatusRequestDto
            {
                LeaveId = leaveId,
                AdminEmployeeNumber = 99999,
                Action = "Approved"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*Employee with Employee Number 99999 not found*");
        }

        [Fact]
        public async Task Handle_InvalidAction_ThrowsBadRequestException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var leaveId = Guid.NewGuid();

            context.Employees.Add(new Employees
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeNumber = 18003,
                FirstName = "Admin3",
                LastName = "User3",
                Email = "admin3@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            context.leaveRequest.Add(new LeaveRequests
            {
                LeaveId = leaveId,
                EmployeeId = Guid.NewGuid(),
                Type = "Casual",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(1),
                Status = "Pending"
            });
            await context.SaveChangesAsync();

            var leaveRepo = DbContextHelper.CreateRepository<LeaveRequests>(context);
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<UpdateLeaveStatusHandler>();

            var handler = new UpdateLeaveStatusHandler(
                leaveRepo, employeeRepo, unitOfWork, logger, _mockKafkaProducer.Object);

            var request = new UpdateLeaveStatusRequestDto
            {
                LeaveId = leaveId,
                AdminEmployeeNumber = 18003,
                Action = "UnknownAction"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("*Must be 'Approved' or 'Rejected'*");
        }
    }
}
