using AttendancePayrollService.Controllers;
using AttendancePayrollService.Domains;
using AttendancePayrollService.Tests.TestHelpers;
using AttendanceService.Domains;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AttendancePayrollService.Tests.Controllers
{
    public class AttendanceControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly AttendanceController _controller;

        public AttendanceControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            var logger = DbContextHelper.CreateLogger<AttendanceController>();
            _controller = new AttendanceController(_mockMediator.Object, logger);
        }

        [Fact]
        public async Task CheckInOut_WhenSuccess_ReturnsOkResult()
        {
            // Arrange
            var request = new CheckInOutRequestDto { EmployeeNumber = 101, CheckInAt = DateTime.UtcNow };
            var expectedResponse = new CheckInOutResponseDto { Success = true, Message = "Applied successfully" };

            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CheckInOut(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task CheckInOut_WhenFailed_ReturnsBadRequestResult()
        {
            // Arrange
            var request = new CheckInOutRequestDto { EmployeeNumber = 101, CheckInAt = DateTime.UtcNow };
            var expectedResponse = new CheckInOutResponseDto { Success = false, Message = "Failed" };

            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CheckInOut(request);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task GetAttendanceRecords_WhenSuccess_ReturnsOkResult()
        {
            // Arrange
            var employeeNumber = 101;
            var expectedResponse = new GetAttendanceRecordsResponseDto
            {
                Success = true,
                Message = "Retrieved",
                Records = new List<AttendancePayrollService.Domains.AttendanceRecordDto>()
            };

            _mockMediator.Setup(m => m.Send(It.Is<GetAttendanceRecordsRequestDto>(r => r.EmployeeNumber == employeeNumber), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetAttendanceRecords(employeeNumber);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task GetAttendanceRecords_WhenFailed_ReturnsBadRequestResult()
        {
            // Arrange
            var employeeNumber = 101;
            var expectedResponse = new GetAttendanceRecordsResponseDto
            {
                Success = false,
                Message = "No records"
            };

            _mockMediator.Setup(m => m.Send(It.Is<GetAttendanceRecordsRequestDto>(r => r.EmployeeNumber == employeeNumber), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetAttendanceRecords(employeeNumber);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task GetPendingAttendanceRecords_ReturnsOkWithResponse()
        {
            // Arrange
            var employeeNumber = 501;
            var expectedResponse = new GetPendingAttendanceRecordsResponseDto
            {
                Success = true,
                Message = "Pending records",
                Records = new List<AttendanceService.Domains.AttendanceRecordDto>()
            };

            _mockMediator.Setup(m => m.Send(It.Is<GetPendingAttendanceRecordsRequestDto>(r => r.EmployeeNumber == employeeNumber), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetPendingAttendanceRecords(employeeNumber);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task UpdateAttendanceStatus_WhenSuccess_ReturnsOkResult()
        {
            // Arrange
            var request = new UpdateAttendanceStatusRequestDto
            {
                RecordId = Guid.NewGuid(),
                EmployeeNumber = 101,
                Action = "Approve",
                AdminUserName = "admin"
            };
            var expectedResponse = new UpdateAttendanceStatusResponseDto
            {
                Success = true,
                Message = "Attendance record Approved successfully"
            };

            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateAttendanceStatus(request, CancellationToken.None);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task UpdateAttendanceStatus_WhenFailed_ReturnsBadRequestResult()
        {
            // Arrange
            var request = new UpdateAttendanceStatusRequestDto
            {
                RecordId = Guid.NewGuid(),
                EmployeeNumber = 101,
                Action = "Invalid",
                AdminUserName = "admin"
            };
            var expectedResponse = new UpdateAttendanceStatusResponseDto
            {
                Success = false,
                Message = "Failed"
            };

            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateAttendanceStatus(request, CancellationToken.None);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().BeEquivalentTo(expectedResponse);
        }
    }
}
