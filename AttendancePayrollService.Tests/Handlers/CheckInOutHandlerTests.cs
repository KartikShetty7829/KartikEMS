using AttendancePayrollService.Businesshandler;
using AttendancePayrollService.Domains;
using AttendancePayrollService.Entity;
using AttendancePayrollService.Exceptions;
using AttendancePayrollService.Interfaces;
using AttendancePayrollService.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AttendancePayrollService.Tests.Handlers
{
    public class CheckInOutHandlerTests
    {
        private readonly Mock<IEmployeeServiceClient> _mockEmployeeServiceClient;

        public CheckInOutHandlerTests()
        {
            _mockEmployeeServiceClient = new Mock<IEmployeeServiceClient>();
        }

        [Fact]
        public async Task Handle_ValidRequest_CreatesAttendanceRecordAndReturnsSuccess()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var attendanceRepo = DbContextHelper.CreateRepository<AttendanceRecord>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<CheckInOutHandler>();

            var employeeId = Guid.NewGuid();
            var employeeNumber = 1001;

            _mockEmployeeServiceClient
                .Setup(c => c.GetEmployeeIdByNumberAsync(employeeNumber))
                .ReturnsAsync(employeeId);

            var handler = new CheckInOutHandler(
                attendanceRepo,
                logger,
                unitOfWork,
                _mockEmployeeServiceClient.Object);

            var checkInTime = DateTime.UtcNow;
            var checkOutTime = checkInTime.AddHours(8);

            var request = new CheckInOutRequestDto
            {
                EmployeeNumber = employeeNumber,
                CheckInAt = checkInTime,
                CheckOutAt = checkOutTime
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Applied successfully");

            var savedRecord = await context.AttendanceRecords.FirstOrDefaultAsync(a => a.EmployeeId == employeeId);
            savedRecord.Should().NotBeNull();
            savedRecord!.CheckInAt.Should().Be(checkInTime);
            savedRecord.CheckOutAt.Should().Be(checkOutTime);
            savedRecord.Status.Should().Be("Pending");
        }

        [Fact]
        public async Task Handle_EmployeeNotFound_ThrowsNotFoundException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var attendanceRepo = DbContextHelper.CreateRepository<AttendanceRecord>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<CheckInOutHandler>();

            var employeeNumber = 9999;
            _mockEmployeeServiceClient
                .Setup(c => c.GetEmployeeIdByNumberAsync(employeeNumber))
                .ReturnsAsync((Guid?)null);

            var handler = new CheckInOutHandler(
                attendanceRepo,
                logger,
                unitOfWork,
                _mockEmployeeServiceClient.Object);

            var request = new CheckInOutRequestDto
            {
                EmployeeNumber = employeeNumber,
                CheckInAt = DateTime.UtcNow,
                CheckOutAt = DateTime.UtcNow.AddHours(8)
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Employee not found.");
        }

        [Fact]
        public async Task Handle_AttendanceAlreadyRecordedForToday_ThrowsBadRequestException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var employeeId = Guid.NewGuid();
            var employeeNumber = 1002;
            var today = DateTime.UtcNow.Date.AddHours(9);

            context.AttendanceRecords.Add(new AttendanceRecord
            {
                RecordId = Guid.NewGuid(),
                EmployeeId = employeeId,
                CheckInAt = today,
                CheckOutAt = today.AddHours(8),
                Status = "Pending"
            });
            await context.SaveChangesAsync();

            var attendanceRepo = DbContextHelper.CreateRepository<AttendanceRecord>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<CheckInOutHandler>();

            _mockEmployeeServiceClient
                .Setup(c => c.GetEmployeeIdByNumberAsync(employeeNumber))
                .ReturnsAsync(employeeId);

            var handler = new CheckInOutHandler(
                attendanceRepo,
                logger,
                unitOfWork,
                _mockEmployeeServiceClient.Object);

            var request = new CheckInOutRequestDto
            {
                EmployeeNumber = employeeNumber,
                CheckInAt = DateTime.UtcNow.Date.AddHours(10), // same date
                CheckOutAt = DateTime.UtcNow.Date.AddHours(18)
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("Attendance already recorded for today.");
        }

        [Fact]
        public async Task Handle_DifferentDayAttendance_AllowsNewRecord()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var employeeId = Guid.NewGuid();
            var employeeNumber = 1003;
            var yesterday = DateTime.UtcNow.Date.AddDays(-1).AddHours(9);

            context.AttendanceRecords.Add(new AttendanceRecord
            {
                RecordId = Guid.NewGuid(),
                EmployeeId = employeeId,
                CheckInAt = yesterday,
                CheckOutAt = yesterday.AddHours(8),
                Status = "Approved"
            });
            await context.SaveChangesAsync();

            var attendanceRepo = DbContextHelper.CreateRepository<AttendanceRecord>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<CheckInOutHandler>();

            _mockEmployeeServiceClient
                .Setup(c => c.GetEmployeeIdByNumberAsync(employeeNumber))
                .ReturnsAsync(employeeId);

            var handler = new CheckInOutHandler(
                attendanceRepo,
                logger,
                unitOfWork,
                _mockEmployeeServiceClient.Object);

            var todayCheckIn = DateTime.UtcNow.Date.AddHours(9);
            var request = new CheckInOutRequestDto
            {
                EmployeeNumber = employeeNumber,
                CheckInAt = todayCheckIn,
                CheckOutAt = todayCheckIn.AddHours(8)
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Applied successfully");
            context.AttendanceRecords.Count().Should().Be(2);
        }
    }
}
