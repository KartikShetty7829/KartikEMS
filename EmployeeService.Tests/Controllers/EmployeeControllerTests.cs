using EmployeeService.domains;
using EmployeeService.domains.EmployeeServiceDepartmentService.domains;
using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Common;
using EmployeeServiceDepartmentService.Controllers;
using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Domains;
using EmployeeServiceDepartmentService.Entity;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EmployeeService.Tests.Controllers
{
    public class EmployeeControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly EmpDepartmentDbContext _context;
        private readonly EmployeeController _controller;

        public EmployeeControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockEnv.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

            _context = DbContextHelper.CreateInMemoryDbContext();
            var logger = DbContextHelper.CreateLogger<EmployeeController>();

            _controller = new EmployeeController(_mockMediator.Object, logger, _mockEnv.Object, _context);
        }

        private static EmployeeProfileResponseDto CreateSampleProfile(int employeeNumber = 101, Guid? employeeId = null)
        {
            return new EmployeeProfileResponseDto
            {
                EmployeeId = employeeId ?? Guid.NewGuid(),
                EmployeeNumber = employeeNumber,
                FullName = "Sample User",
                Email = "user@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true,
                AddressLine1 = "123 Main St",
                City = "New York",
                State = "NY",
                PostalCode = "10001",
                Country = "USA",
                AddressType = "Home"
            };
        }

        [Fact]
        public async Task AddEmployee_WhenNullPayload_ReturnsBadRequest()
        {
            var result = await _controller.AddEmployee(null!);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddEmployee_WhenFailed_ReturnsBadRequest()
        {
            var request = new EmployeeRequestDto
            {
                FirstName = "Fail",
                LastName = "User",
                Email = "fail@test.com",
                Phone = "12345",
                HireDate = DateTime.UtcNow,
                Status = true
            };
            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeResponseDto { Success = false, Message = "Failed" });

            var result = await _controller.AddEmployee(request);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddEmployee_WhenSuccess_ReturnsOk()
        {
            var request = new EmployeeRequestDto
            {
                FirstName = "Success",
                LastName = "User",
                Email = "success@test.com",
                Phone = "12345",
                HireDate = DateTime.UtcNow,
                Status = true
            };
            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeResponseDto { Success = true, EmployeeId = Guid.NewGuid() });

            var result = await _controller.AddEmployee(request);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateHireDate_WhenNull_ReturnsBadRequest()
        {
            var result = await _controller.UpdateHireDate(null!);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateHireDate_WhenNotFound_ReturnsNotFound()
        {
            var request = new EmployeeUpdateRequestDto { EmployeeNumber = 101, HireDate = DateTime.UtcNow };
            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeUpdateResponseDto { Success = false });

            var result = await _controller.UpdateHireDate(request);
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task UpdateHireDate_WhenSuccess_ReturnsOk()
        {
            var request = new EmployeeUpdateRequestDto { EmployeeNumber = 101, HireDate = DateTime.UtcNow };
            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeUpdateResponseDto { Success = true });

            var result = await _controller.UpdateHireDate(request);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateEmployee_WhenFailed_ReturnsBadRequest()
        {
            var request = new EmployeeRecordUpdateRequestDto { EmployeeNumber = 101 };
            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeRecordUpdateResponseDto { Success = false });

            var result = await _controller.UpdateEmployee(request);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateEmployee_WhenSuccess_ReturnsOk()
        {
            var request = new EmployeeRecordUpdateRequestDto { EmployeeNumber = 101 };
            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeRecordUpdateResponseDto { Success = true });

            var result = await _controller.UpdateEmployee(request);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetEmployeeProfile_WhenNotFound_ReturnsNotFound()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<EmployeeProfileRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EmployeeProfileResponseDto)null!);

            var result = await _controller.GetEmployeeProfile(101);
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetEmployeeProfile_WhenFound_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<EmployeeProfileRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateSampleProfile(101));

            var result = await _controller.GetEmployeeProfile(101);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateEmployeeStatus_WhenFailed_ReturnsNotFound()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<EmployeeRecordDeleteRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeRecordDeleteResponseDto { Success = false });

            var result = await _controller.UpdateEmployeeStatus(101, EmployeeStatus.Inactive);
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task UpdateEmployeeStatus_WhenSuccess_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<EmployeeRecordDeleteRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeRecordDeleteResponseDto { Success = true });

            var result = await _controller.UpdateEmployeeStatus(101, EmployeeStatus.Active);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllEmployees_WhenEmpty_ReturnsNotFound()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllEmployeeRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAllEmployeeResponseDto { Employees = new List<EmployeeDto>() });

            var result = await _controller.GetAllEmployees();
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetAllEmployees_WhenPopulated_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllEmployeeRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAllEmployeeResponseDto
                {
                    Employees = new List<EmployeeDto> { new EmployeeDto { FullName = "John Doe" } }
                });

            var result = await _controller.GetAllEmployees();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateDesignation_WhenNull_ReturnsBadRequest()
        {
            var result = await _controller.CreateDesignation(null!);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateDesignation_WhenSuccess_ReturnsOk()
        {
            var request = new CreateDesignationsRequestDto
            {
                Designations = new List<SingleDesignationDto> { new SingleDesignationDto { Title = "Manager" } }
            };
            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateDesignationResponseDto { Success = true });

            var result = await _controller.CreateDesignation(request);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AssignDesignation_WhenSuccess_ReturnsOk()
        {
            var request = new AssignDesignationRequestDto { EmployeeNumber = 101, DesignationName = "Lead" };
            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AssignDesignationResponseDto { Success = true });

            var result = await _controller.AssignDesignation(request);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AddAddress_WhenInvalid_ReturnsBadRequest()
        {
            var result = await _controller.AddAddress(null!);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddAddress_WhenSuccess_ReturnsOk()
        {
            var request = new AddEmployeeAddressesRequestDto
            {
                Employees = new List<EmployeeAddressesDto>
                {
                    new EmployeeAddressesDto
                    {
                        EmployeeNumber = 101,
                        Addresses = new List<AddressDto>
                        {
                            new AddressDto
                            {
                                AddressLine1 = "123 Main St",
                                City = "New York",
                                State = "NY",
                                PostalCode = "10001",
                                Country = "USA",
                                AddressType = "Home"
                            }
                        }
                    }
                }
            };
            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AddEmployeeAddressesResponseDto { Success = true });

            var result = await _controller.AddAddress(request);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AddProject_WhenSuccess_ReturnsOk()
        {
            var request = new AddProjectRequestDto { Name = "AI Project" };
            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AddProjectResponseDto { Success = true });

            var result = await _controller.AddProject(request);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AssignEmployeeToProject_WhenSuccess_ReturnsOk()
        {
            var request = new AssignEmployeeProjectRequestDto { EmployeeNumber = 101, ProjectName = "AI Project" };
            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AssignEmployeeProjectResponseDto { Success = true });

            var result = await _controller.AssignEmployeeToProject(request);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateLeaveStatus_WhenSuccess_ReturnsOk()
        {
            var request = new UpdateLeaveStatusRequestDto { LeaveId = Guid.NewGuid(), Action = "Approved" };
            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateLeaveStatusResponseDto { Success = true, Message = "Approved" });

            var result = await _controller.UpdateLeaveStatus(request);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetEmployeesWithPendingLeaves_WhenFound_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetEmployeesWithPendingLeavesRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetEmployeesWithPendingLeavesResponseDto
                {
                    PendingLeaves = new List<EmployeePendingLeaveDto>
                    {
                        new EmployeePendingLeaveDto
                        {
                            EmployeeName = "Emp 1",
                            Type = "Sick",
                            Status = "Pending",
                            ApprovedByName = "Manager"
                        }
                    }
                });

            var result = await _controller.GetEmployeesWithPendingLeaves(101, CancellationToken.None);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ApplyLeave_WhenSuccess_ReturnsOk()
        {
            var request = new ApplyLeaveRequestDto { EmployeeNumber = 101, Type = "Annual" };
            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApplyLeaveResponseDto { Success = true });

            var result = await _controller.ApplyLeave(request);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateEmployeeRecords_WhenSuccess_ReturnsOk()
        {
            var request = new UpdateEmployeeProfileRequestDto { EmployeeNumber = 101 };
            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateEmployeeProfileResponseDto { Success = true });

            var result = await _controller.UpdateEmployeeRecords(request);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetEmployeeProjects_WhenFound_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<EmployeeProjectRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeProjectResponseDto());

            var result = await _controller.GetEmployeeProjects(101);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetEmployeeDesignation_WhenFound_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<EmployeeDesignationRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeDesignationResponseDto());

            var result = await _controller.GetEmployeeDesignation(101);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetEmployeeLeaves_WhenLeavesExist_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetEmployeeLeavesRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetEmployeeLeavesResponseDto
                {
                    Leaves = new List<EmployeeLeaveDto> { new EmployeeLeaveDto { Type = "Sick" } }
                });

            var result = await _controller.GetEmployeeLeaves(101, CancellationToken.None);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetEmployeeById_WhenFound_ReturnsOk()
        {
            var empId = Guid.NewGuid();
            _mockMediator.Setup(m => m.Send(It.IsAny<EmployeeProfileByIdRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateSampleProfile(101, empId));

            var result = await _controller.GetEmployeeById(empId);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetEmployeesByManager_ReturnsOk()
        {
            var managerId = Guid.NewGuid();
            _mockMediator.Setup(m => m.Send(It.IsAny<GetEmployeesByManagerRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ReportingEmployeeDto>());

            var result = await _controller.GetEmployeesByManager(managerId);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreatePayrollRecords_WhenSuccess_ReturnsOk()
        {
            var request = new CreatePayrollRecordsRequestDto { EmployeeNumber = 101 };
            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreatePayrollRecordsResponseDto { Success = true });

            var result = await _controller.CreatePayrollRecords(request);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPayrollRecords_WhenSuccess_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPayrollRecordsRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetPayrollRecordsResponseDto { Success = true });

            var result = await _controller.GetPayrollRecords(101);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetNotifications_WhenEmployeeExists_ReturnsNotifications()
        {
            var emp = new Employees
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeNumber = 5005,
                FirstName = "Test",
                LastName = "User",
                Email = "test5005@example.com",
                Phone = "12345",
                HireDate = DateTime.UtcNow
            };
            _context.Employees.Add(emp);
            _context.Notifications.Add(new Notification
            {
                NotificationId = Guid.NewGuid(),
                EmployeeId = emp.EmployeeId,
                Message = "Hello",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            var result = await _controller.GetNotifications(5005);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteNotification_RemovesNotificationAndReturnsOk()
        {
            var notifId = Guid.NewGuid();
            _context.Notifications.Add(new Notification
            {
                NotificationId = notifId,
                EmployeeId = Guid.NewGuid(),
                Message = "Bye",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteNotification(notifId);
            result.Should().BeOfType<OkResult>();

            var exists = await _context.Notifications.FindAsync(notifId);
            exists.Should().BeNull();
        }
    }
}
