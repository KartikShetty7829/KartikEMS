using AttendancePayrollService.DataAccess;
using AttendancePayrollService.Entity;
using AttendancePayrollService.externalservicereponse;
using AttendancePayrollService.Interfaces;
using AttendancePayrollService.Tests.TestHelpers;
using AttendanceService.Businesshandler;
using AttendanceService.Domains;
using FluentAssertions;
using Moq;
using Xunit;

namespace AttendancePayrollService.Tests.Handlers
{
    public class GetPendingAttendanceRecordsHandlerTests
    {
        private readonly Mock<IEmployeeServiceClient> _mockEmployeeServiceClient;

        public GetPendingAttendanceRecordsHandlerTests()
        {
            _mockEmployeeServiceClient = new Mock<IEmployeeServiceClient>();
        }

        [Fact]
        public async Task Handle_ValidRequest_WithPendingRecords_ReturnsEnrichedRecords()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var managerId = Guid.NewGuid();
            var managerEmpNumber = 5001;

            var emp1Id = Guid.NewGuid();
            var emp2Id = Guid.NewGuid();

            var teamMembers = new List<EmployeeLookupDto>
            {
                new EmployeeLookupDto
                {
                    EmployeeId = emp1Id,
                    FullName = "Alice Smith",
                    EmployeeNumber = 5002,
                    ManagerId = managerId,
                    Email = "alice@example.com",
                    Phone = "1111111111",
                    Status = true
                },
                new EmployeeLookupDto
                {
                    EmployeeId = emp2Id,
                    FullName = "Bob Jones",
                    EmployeeNumber = 5003,
                    ManagerId = managerId,
                    Email = "bob@example.com",
                    Phone = "2222222222",
                    Status = true
                }
            };

            var record1 = new AttendanceRecord
            {
                RecordId = Guid.NewGuid(),
                EmployeeId = emp1Id,
                CheckInAt = DateTime.UtcNow.Date.AddDays(-1).AddHours(9),
                CheckOutAt = DateTime.UtcNow.Date.AddDays(-1).AddHours(17),
                Status = "Pending"
            };

            var record2 = new AttendanceRecord
            {
                RecordId = Guid.NewGuid(),
                EmployeeId = emp2Id,
                CheckInAt = DateTime.UtcNow.Date.AddHours(9),
                CheckOutAt = DateTime.UtcNow.Date.AddHours(17),
                Status = "Pending"
            };

            var approvedRecord = new AttendanceRecord
            {
                RecordId = Guid.NewGuid(),
                EmployeeId = emp1Id,
                CheckInAt = DateTime.UtcNow.Date.AddDays(-2).AddHours(9),
                CheckOutAt = DateTime.UtcNow.Date.AddDays(-2).AddHours(17),
                Status = "Approved" // should be excluded
            };

            context.AttendanceRecords.AddRange(record1, record2, approvedRecord);
            await context.SaveChangesAsync();

            var attendanceRepo = DbContextHelper.CreateRepository<AttendanceRecord>(context);
            var logger = DbContextHelper.CreateLogger<GetPendingAttendanceRecordsHandler>();

            _mockEmployeeServiceClient
                .Setup(c => c.GetEmployeeIdByNumberAsync(managerEmpNumber))
                .ReturnsAsync(managerId);

            _mockEmployeeServiceClient
                .Setup(c => c.GetEmployeesByManagerIdAsync(managerId))
                .ReturnsAsync(teamMembers);

            var handler = new GetPendingAttendanceRecordsHandler(
                attendanceRepo,
                logger,
                _mockEmployeeServiceClient.Object);

            var request = new GetPendingAttendanceRecordsRequestDto
            {
                EmployeeNumber = managerEmpNumber
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Pending attendance records retrieved successfully.");
            result.Records.Should().HaveCount(2);

            // Newest first
            result.Records![0].FullName.Should().Be("Bob Jones");
            result.Records[0].Status.Should().Be("Pending");
            result.Records[1].FullName.Should().Be("Alice Smith");
            result.Records[1].Status.Should().Be("Pending");
        }

        [Fact]
        public async Task Handle_ManagerNotFound_ReturnsFailureResponse()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var attendanceRepo = DbContextHelper.CreateRepository<AttendanceRecord>(context);
            var logger = DbContextHelper.CreateLogger<GetPendingAttendanceRecordsHandler>();

            var managerEmpNumber = 9999;
            _mockEmployeeServiceClient
                .Setup(c => c.GetEmployeeIdByNumberAsync(managerEmpNumber))
                .ReturnsAsync((Guid?)null);

            var handler = new GetPendingAttendanceRecordsHandler(
                attendanceRepo,
                logger,
                _mockEmployeeServiceClient.Object);

            var request = new GetPendingAttendanceRecordsRequestDto
            {
                EmployeeNumber = managerEmpNumber
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Manager not found.");
            result.Records.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_NoTeamMembersFound_ReturnsFailureResponse()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var managerId = Guid.NewGuid();
            var managerEmpNumber = 5001;

            var attendanceRepo = DbContextHelper.CreateRepository<AttendanceRecord>(context);
            var logger = DbContextHelper.CreateLogger<GetPendingAttendanceRecordsHandler>();

            _mockEmployeeServiceClient
                .Setup(c => c.GetEmployeeIdByNumberAsync(managerEmpNumber))
                .ReturnsAsync(managerId);

            _mockEmployeeServiceClient
                .Setup(c => c.GetEmployeesByManagerIdAsync(managerId))
                .ReturnsAsync(new List<EmployeeLookupDto>());

            var handler = new GetPendingAttendanceRecordsHandler(
                attendanceRepo,
                logger,
                _mockEmployeeServiceClient.Object);

            var request = new GetPendingAttendanceRecordsRequestDto
            {
                EmployeeNumber = managerEmpNumber
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("No team members found.");
            result.Records.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_NoPendingAttendanceRecords_ReturnsFailureResponse()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var managerId = Guid.NewGuid();
            var empId = Guid.NewGuid();
            var managerEmpNumber = 5001;

            var teamMembers = new List<EmployeeLookupDto>
            {
                new EmployeeLookupDto
                {
                    EmployeeId = empId,
                    FullName = "Charlie",
                    EmployeeNumber = 5002,
                    Email = "charlie@example.com",
                    Phone = "3333333333",
                    Status = true
                }
            };

            // Only approved records exist
            context.AttendanceRecords.Add(new AttendanceRecord
            {
                RecordId = Guid.NewGuid(),
                EmployeeId = empId,
                CheckInAt = DateTime.UtcNow.Date.AddHours(9),
                Status = "Approved"
            });
            await context.SaveChangesAsync();

            var attendanceRepo = DbContextHelper.CreateRepository<AttendanceRecord>(context);
            var logger = DbContextHelper.CreateLogger<GetPendingAttendanceRecordsHandler>();

            _mockEmployeeServiceClient
                .Setup(c => c.GetEmployeeIdByNumberAsync(managerEmpNumber))
                .ReturnsAsync(managerId);

            _mockEmployeeServiceClient
                .Setup(c => c.GetEmployeesByManagerIdAsync(managerId))
                .ReturnsAsync(teamMembers);

            var handler = new GetPendingAttendanceRecordsHandler(
                attendanceRepo,
                logger,
                _mockEmployeeServiceClient.Object);

            var request = new GetPendingAttendanceRecordsRequestDto
            {
                EmployeeNumber = managerEmpNumber
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("No pending attendance records found.");
            result.Records.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_TeamMemberDetailsMissing_DefaultsToUnknown()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var managerId = Guid.NewGuid();
            var empId = Guid.NewGuid();
            var managerEmpNumber = 5001;

            var teamMembers = new List<EmployeeLookupDto>
            {
                new EmployeeLookupDto
                {
                    EmployeeId = empId,
                    FullName = null!, // null name
                    EmployeeNumber = 5002,
                    Email = "test@example.com",
                    Phone = "4444444444",
                    Status = true
                }
            };

            context.AttendanceRecords.Add(new AttendanceRecord
            {
                RecordId = Guid.NewGuid(),
                EmployeeId = empId,
                CheckInAt = DateTime.UtcNow,
                Status = "Pending"
            });
            await context.SaveChangesAsync();

            var attendanceRepo = DbContextHelper.CreateRepository<AttendanceRecord>(context);
            var logger = DbContextHelper.CreateLogger<GetPendingAttendanceRecordsHandler>();

            _mockEmployeeServiceClient
                .Setup(c => c.GetEmployeeIdByNumberAsync(managerEmpNumber))
                .ReturnsAsync(managerId);

            _mockEmployeeServiceClient
                .Setup(c => c.GetEmployeesByManagerIdAsync(managerId))
                .ReturnsAsync(teamMembers);

            var handler = new GetPendingAttendanceRecordsHandler(
                attendanceRepo,
                logger,
                _mockEmployeeServiceClient.Object);

            var request = new GetPendingAttendanceRecordsRequestDto
            {
                EmployeeNumber = managerEmpNumber
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Records![0].FullName.Should().Be("Unknown");
        }
    }
}
