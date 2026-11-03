using AttendancePayrollService.Businesshandler;
using AttendancePayrollService.Domains;
using AttendancePayrollService.Entity;
using AttendancePayrollService.Exceptions;
using AttendancePayrollService.Interfaces;
using AttendancePayrollService.Tests.TestHelpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace AttendancePayrollService.Tests.Handlers
{
    public class GetAttendanceRecordsHandlerTests
    {
        private readonly Mock<IEmployeeServiceClient> _mockEmployeeServiceClient;

        public GetAttendanceRecordsHandlerTests()
        {
            _mockEmployeeServiceClient = new Mock<IEmployeeServiceClient>();
        }

        [Fact]
        public async Task Handle_ValidRequest_WithRecords_ReturnsRecordsInDescendingOrder()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var employeeId = Guid.NewGuid();
            var employeeNumber = 2001;

            var record1 = new AttendanceRecord
            {
                RecordId = Guid.NewGuid(),
                EmployeeId = employeeId,
                CheckInAt = DateTime.UtcNow.Date.AddDays(-2).AddHours(9),
                CheckOutAt = DateTime.UtcNow.Date.AddDays(-2).AddHours(17),
                Status = "Approved",
                ApprovedBy = "Admin",
                ApprovedAt = DateTime.UtcNow.Date.AddDays(-2).AddHours(18)
            };

            var record2 = new AttendanceRecord
            {
                RecordId = Guid.NewGuid(),
                EmployeeId = employeeId,
                CheckInAt = DateTime.UtcNow.Date.AddDays(-1).AddHours(9),
                CheckOutAt = DateTime.UtcNow.Date.AddDays(-1).AddHours(17),
                Status = "Pending"
            };

            context.AttendanceRecords.AddRange(record1, record2);
            await context.SaveChangesAsync();

            var attendanceRepo = DbContextHelper.CreateRepository<AttendanceRecord>(context);
            var logger = DbContextHelper.CreateLogger<GetAttendanceRecordsHandler>();

            _mockEmployeeServiceClient
                .Setup(c => c.GetEmployeeIdByNumberAsync(employeeNumber))
                .ReturnsAsync(employeeId);

            var handler = new GetAttendanceRecordsHandler(
                attendanceRepo,
                logger,
                _mockEmployeeServiceClient.Object);

            var request = new GetAttendanceRecordsRequestDto
            {
                EmployeeNumber = employeeNumber
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Attendance records retrieved successfully.");
            result.Records.Should().HaveCount(2);
            result.Records![0].CheckInAt.Should().Be(record2.CheckInAt); // newer first
            result.Records[1].CheckInAt.Should().Be(record1.CheckInAt);
            result.Records[1].ApprovedBy.Should().Be("Admin");
        }

        [Fact]
        public async Task Handle_EmployeeNotFound_ThrowsNotFoundException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var attendanceRepo = DbContextHelper.CreateRepository<AttendanceRecord>(context);
            var logger = DbContextHelper.CreateLogger<GetAttendanceRecordsHandler>();

            var employeeNumber = 9999;
            _mockEmployeeServiceClient
                .Setup(c => c.GetEmployeeIdByNumberAsync(employeeNumber))
                .ReturnsAsync((Guid?)null);

            var handler = new GetAttendanceRecordsHandler(
                attendanceRepo,
                logger,
                _mockEmployeeServiceClient.Object);

            var request = new GetAttendanceRecordsRequestDto
            {
                EmployeeNumber = employeeNumber
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Employee not found.");
        }

        [Fact]
        public async Task Handle_NoRecordsInDatabase_ReturnsFailureResponse()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var attendanceRepo = DbContextHelper.CreateRepository<AttendanceRecord>(context);
            var logger = DbContextHelper.CreateLogger<GetAttendanceRecordsHandler>();

            var employeeId = Guid.NewGuid();
            var employeeNumber = 2002;

            _mockEmployeeServiceClient
                .Setup(c => c.GetEmployeeIdByNumberAsync(employeeNumber))
                .ReturnsAsync(employeeId);

            var handler = new GetAttendanceRecordsHandler(
                attendanceRepo,
                logger,
                _mockEmployeeServiceClient.Object);

            var request = new GetAttendanceRecordsRequestDto
            {
                EmployeeNumber = employeeNumber
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("No attendance records found.");
            result.Records.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_DoesNotReturnOtherEmployeesRecords()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var targetEmployeeId = Guid.NewGuid();
            var otherEmployeeId = Guid.NewGuid();
            var targetEmpNumber = 2003;

            context.AttendanceRecords.Add(new AttendanceRecord
            {
                RecordId = Guid.NewGuid(),
                EmployeeId = otherEmployeeId,
                CheckInAt = DateTime.UtcNow.Date.AddHours(9),
                Status = "Pending"
            });
            await context.SaveChangesAsync();

            var attendanceRepo = DbContextHelper.CreateRepository<AttendanceRecord>(context);
            var logger = DbContextHelper.CreateLogger<GetAttendanceRecordsHandler>();

            _mockEmployeeServiceClient
                .Setup(c => c.GetEmployeeIdByNumberAsync(targetEmpNumber))
                .ReturnsAsync(targetEmployeeId);

            var handler = new GetAttendanceRecordsHandler(
                attendanceRepo,
                logger,
                _mockEmployeeServiceClient.Object);

            var request = new GetAttendanceRecordsRequestDto
            {
                EmployeeNumber = targetEmpNumber
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("No attendance records found.");
        }
    }
}
