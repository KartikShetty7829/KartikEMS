using AttendancePayrollService.DataAccess;
using AttendancePayrollService.Entity;
using AttendancePayrollService.Exceptions;
using AttendancePayrollService.Interfaces;
using AttendancePayrollService.Tests.TestHelpers;
using AttendanceService.Businesshandler;
using AttendanceService.Domains;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AttendancePayrollService.Tests.Handlers
{
    public class UpdateAttendanceStatusHandlerTests
    {
        private readonly Mock<IEmployeeServiceClient> _mockEmployeeServiceClient;

        public UpdateAttendanceStatusHandlerTests()
        {
            _mockEmployeeServiceClient = new Mock<IEmployeeServiceClient>();
        }

        [Fact]
        public async Task Handle_ValidApproveRequest_UpdatesStatusToApprovedAndReturnsSuccess()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var recordId = Guid.NewGuid();
            var employeeId = Guid.NewGuid();
            var approverId = Guid.NewGuid();
            var approverEmployeeNumber = 10001;
            var adminUsername = "admin_jane";

            context.AttendanceRecords.Add(new AttendanceRecord
            {
                RecordId = recordId,
                EmployeeId = employeeId,
                CheckInAt = DateTime.UtcNow.Date.AddHours(9),
                CheckOutAt = DateTime.UtcNow.Date.AddHours(17),
                Status = "Pending"
            });
            await context.SaveChangesAsync();

            var attendanceRepo = DbContextHelper.CreateRepository<AttendanceRecord>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<UpdateAttendanceStatusHandler>();

            _mockEmployeeServiceClient
                .Setup(c => c.GetEmployeeIdByNumberAsync(approverEmployeeNumber))
                .ReturnsAsync(approverId);

            var handler = new UpdateAttendanceStatusHandler(
                attendanceRepo,
                unitOfWork,
                logger,
                _mockEmployeeServiceClient.Object);

            var request = new UpdateAttendanceStatusRequestDto
            {
                RecordId = recordId,
                EmployeeNumber = approverEmployeeNumber,
                AdminUserName = adminUsername,
                Action = "Approve"
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Attendance record Approved successfully");

            var updatedRecord = await context.AttendanceRecords.FirstOrDefaultAsync(a => a.RecordId == recordId);
            updatedRecord.Should().NotBeNull();
            updatedRecord!.Status.Should().Be("Approved");
            updatedRecord.ApprovedBy.Should().Be(adminUsername);
            updatedRecord.ApproverId.Should().Be(approverId);
            updatedRecord.ApprovedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ValidRejectRequest_UpdatesStatusToRejectedAndReturnsSuccess()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var recordId = Guid.NewGuid();
            var employeeId = Guid.NewGuid();
            var approverId = Guid.NewGuid();
            var approverEmployeeNumber = 10002;
            var adminUsername = "admin_bob";

            context.AttendanceRecords.Add(new AttendanceRecord
            {
                RecordId = recordId,
                EmployeeId = employeeId,
                CheckInAt = DateTime.UtcNow.Date.AddHours(9),
                CheckOutAt = DateTime.UtcNow.Date.AddHours(17),
                Status = "Pending"
            });
            await context.SaveChangesAsync();

            var attendanceRepo = DbContextHelper.CreateRepository<AttendanceRecord>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<UpdateAttendanceStatusHandler>();

            _mockEmployeeServiceClient
                .Setup(c => c.GetEmployeeIdByNumberAsync(approverEmployeeNumber))
                .ReturnsAsync(approverId);

            var handler = new UpdateAttendanceStatusHandler(
                attendanceRepo,
                unitOfWork,
                logger,
                _mockEmployeeServiceClient.Object);

            var request = new UpdateAttendanceStatusRequestDto
            {
                RecordId = recordId,
                EmployeeNumber = approverEmployeeNumber,
                AdminUserName = adminUsername,
                Action = "Reject"
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Attendance record Rejected successfully");

            var updatedRecord = await context.AttendanceRecords.FirstOrDefaultAsync(a => a.RecordId == recordId);
            updatedRecord.Should().NotBeNull();
            updatedRecord!.Status.Should().Be("Rejected");
            updatedRecord.ApprovedBy.Should().Be(adminUsername);
            updatedRecord.ApproverId.Should().Be(approverId);
            updatedRecord.ApprovedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_RecordNotFound_ThrowsNotFoundException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var attendanceRepo = DbContextHelper.CreateRepository<AttendanceRecord>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<UpdateAttendanceStatusHandler>();

            var nonExistentRecordId = Guid.NewGuid();

            var handler = new UpdateAttendanceStatusHandler(
                attendanceRepo,
                unitOfWork,
                logger,
                _mockEmployeeServiceClient.Object);

            var request = new UpdateAttendanceStatusRequestDto
            {
                RecordId = nonExistentRecordId,
                EmployeeNumber = 10001,
                AdminUserName = "admin",
                Action = "Approve"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Attendance record with ID {nonExistentRecordId} not found");
        }

        [Theory]
        [InlineData("Cancel")]
        [InlineData("Pending")]
        [InlineData("Invalid")]
        [InlineData("")]
        public async Task Handle_InvalidAction_ThrowsBadRequestException(string invalidAction)
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var recordId = Guid.NewGuid();

            context.AttendanceRecords.Add(new AttendanceRecord
            {
                RecordId = recordId,
                EmployeeId = Guid.NewGuid(),
                CheckInAt = DateTime.UtcNow,
                Status = "Pending"
            });
            await context.SaveChangesAsync();

            var attendanceRepo = DbContextHelper.CreateRepository<AttendanceRecord>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<UpdateAttendanceStatusHandler>();

            var handler = new UpdateAttendanceStatusHandler(
                attendanceRepo,
                unitOfWork,
                logger,
                _mockEmployeeServiceClient.Object);

            var request = new UpdateAttendanceStatusRequestDto
            {
                RecordId = recordId,
                EmployeeNumber = 10001,
                AdminUserName = "admin",
                Action = invalidAction
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("Invalid action. Must be 'Approve' or 'Reject'.");
        }

        [Fact]
        public async Task Handle_ApproverEmployeeNotFound_ThrowsNotFoundException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var recordId = Guid.NewGuid();
            var approverEmployeeNumber = 99999;

            context.AttendanceRecords.Add(new AttendanceRecord
            {
                RecordId = recordId,
                EmployeeId = Guid.NewGuid(),
                CheckInAt = DateTime.UtcNow,
                Status = "Pending"
            });
            await context.SaveChangesAsync();

            var attendanceRepo = DbContextHelper.CreateRepository<AttendanceRecord>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<UpdateAttendanceStatusHandler>();

            _mockEmployeeServiceClient
                .Setup(c => c.GetEmployeeIdByNumberAsync(approverEmployeeNumber))
                .ReturnsAsync((Guid?)null);

            var handler = new UpdateAttendanceStatusHandler(
                attendanceRepo,
                unitOfWork,
                logger,
                _mockEmployeeServiceClient.Object);

            var request = new UpdateAttendanceStatusRequestDto
            {
                RecordId = recordId,
                EmployeeNumber = approverEmployeeNumber,
                AdminUserName = "admin",
                Action = "Approve"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Employee not found for Employee Number {approverEmployeeNumber}");
        }
    }
}
