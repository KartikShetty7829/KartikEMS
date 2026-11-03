using Asp.Versioning;
using EmployeeService.domains;
using EmployeeService.domains.EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Common;
using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Domains;
using EmployeeServiceDepartmentService.Exceptions;
using EmployeeServiceDepartmentService.interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace EmployeeServiceDepartmentService.Controllers
{
    /// <summary>
    /// Thin controller — only routes requests and returns responses (SRP).
    /// Email/SMS notification logic moved to handlers or dedicated services.
    /// Depends on interfaces, not concrete classes (DIP).
    /// Protected with ASP.NET Core native rate limiting policies.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [EnableRateLimiting("EmployeeGeneralPolicy")]
    public class EmployeeController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EmployeeController> _logger;

        private readonly IWebHostEnvironment _env;
        private readonly EmpDepartmentDbContext _dbContext;

        public EmployeeController(
            IMediator mediator,
            ILogger<EmployeeController> logger, 
            IWebHostEnvironment env,
            EmpDepartmentDbContext dbContext)
        {
            _mediator = mediator;
            _logger = logger;
            _env = env;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Adds a new employee and dispatches welcome email (SMTP) & SMS (Twilio).
        /// Rate-limited via NotificationPolicy (3 req/min) to prevent billing surges.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("NotificationPolicy")]
        [HttpPost("add")]
        public async Task<ActionResult> AddEmployee([FromBody] EmployeeRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "Invalid request payload." });
            }

            var response = await _mediator.Send(request);
            if (!response.Success)
            {
                _logger.LogWarning("Failed to add employee: {Message}", response.Message);
                return BadRequest(response);
            }

            _logger.LogInformation("Employee added successfully with Id {EmployeeId}", response.EmployeeId);

            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("update-hiredate")]
        public async Task<IActionResult> UpdateHireDate([FromBody] EmployeeUpdateRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request payload.");
            }

            _logger.LogInformation("Received request to update HireDate for employee {EmployeeNumber}", request.EmployeeNumber);

            var response = await _mediator.Send(request);

            if (!response.Success)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update")]
        public async Task<ActionResult> UpdateEmployee([FromBody] EmployeeRecordUpdateRequestDto request)
        {
            _logger.LogInformation("Received update request for employee {EmployeeNumber}", request.EmployeeNumber);

            var response = await _mediator.Send(request);

            if (!response.Success)
            {
                _logger.LogWarning("Update failed for employee {EmployeeNumber}: {Message}", request.EmployeeNumber, response.Message);
                return BadRequest(response);
            }

            _logger.LogInformation("Update succeeded for employee {EmployeeNumber}", request.EmployeeNumber);
            return Ok(response);
        }

        [HttpGet("employeeNumber")]
        public async Task<IActionResult> GetEmployeeProfile(int employeeNumber)
        {
            _logger.LogInformation("API Request: Fetching profile for EmployeeNumber {EmployeeNumber}", employeeNumber);
            var result = await _mediator.Send(new EmployeeProfileRequestDto
            {
                EmployeeNumber = employeeNumber
            });

            if (result == null)
            {
                _logger.LogWarning("Employee profile not found for EmployeeNumber {EmployeeNumber}", employeeNumber);
                return NotFound(new { Message = "Employee not found" });
            }
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateEmployeeStatus")]
        public async Task<IActionResult> UpdateEmployeeStatus(int employeeNumber, [FromBody] EmployeeStatus status)
        {
            var request = new EmployeeRecordDeleteRequestDto
            {
                EmployeeNumber = employeeNumber,
                Status = status
            };

            var response = await _mediator.Send(request);

            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllEmployees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            var response = await _mediator.Send(new GetAllEmployeeRequestDto());

            if (response.Employees == null || !response.Employees.Any())
                return NotFound("No employees found");

            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("CreateDesignation")]
        public async Task<IActionResult> CreateDesignation([FromBody] CreateDesignationsRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { Success = false, Message = "Invalid request payload" });
            }

            var response = await _mediator.Send(request);

            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        /// <summary>
        /// Assign a designation to an employee based on EmployeeNumber and DesignationName.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("AssignDesignation")]
        public async Task<IActionResult> AssignDesignation([FromBody] AssignDesignationRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { Success = false, Message = "Invalid request payload" });
            }

            var response = await _mediator.Send(request);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        /// <summary>
        /// Add an address for an employee. Only allowed once per employee.
        /// </summary>
        [HttpPost("AddAddress")]
        public async Task<IActionResult> AddAddress([FromBody] AddEmployeeAddressesRequestDto request)
        {
            if (request == null || request.Employees == null || !request.Employees.Any())
            {
                return BadRequest(new { Success = false, Message = "Invalid request payload" });
            }

            // Ensure each employee has at least one address
            if (request.Employees.Any(e => e.Addresses == null || !e.Addresses.Any()))
            {
                return BadRequest(new { Success = false, Message = "One or more employees have no addresses provided" });
            }

            var response = await _mediator.Send(request);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpPost("AddProject")]
        public async Task<IActionResult> AddProject([FromBody] AddProjectRequestDto request)
        {
            var response = await _mediator.Send(request);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPost("AssignEmployeeToProject")]
        public async Task<IActionResult> AssignEmployeeToProject([FromBody] AssignEmployeeProjectRequestDto request)
        {
            var response = await _mediator.Send(request);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("update-status")]
        public async Task<IActionResult> UpdateLeaveStatus([FromBody] UpdateLeaveStatusRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body cannot be null.");

            var response = await _mediator.Send(request);
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetEmployeesWithPendingLeaves")]
        public async Task<IActionResult> GetEmployeesWithPendingLeaves(
            [FromQuery] int employeeNumber,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "API call: GetEmployeesWithPendingLeaves for EmployeeNumber: {EmployeeNumber}",
                employeeNumber);

            var request = new GetEmployeesWithPendingLeavesRequestDto
            {
                EmployeeNumber = employeeNumber
            };

            var response = await _mediator.Send(request, cancellationToken);

            if (response.PendingLeaves == null || !response.PendingLeaves.Any())
            {
                return NotFound("No pending leave requests found.");
            }

            return Ok(response);
        }

        /// <summary>
        /// Apply for leave, which dispatches email and SMS notifications.
        /// Rate-limited via NotificationPolicy (3 req/min) to prevent spamming notification channels.
        /// </summary>
        [EnableRateLimiting("NotificationPolicy")]
        [HttpPost("apply")]
        public async Task<IActionResult> ApplyLeave([FromBody] ApplyLeaveRequestDto request)
        {
            if (request == null)
                return BadRequest(new ApplyLeaveResponseDto
                {
                    Success = false,
                    Message = "Invalid leave request payload."
                });

            var response = await _mediator.Send(request);

            if (!response.Success)
            {
                _logger.LogWarning("Failed to apply leave: {Message}", response.Message);
                return BadRequest(response);
            }

            _logger.LogInformation("Leave applied successfully for Employee {EmployeeNumber}", request.EmployeeNumber);

            return Ok(response);
        }

        [HttpPut("update-recordsforemployee")]
        public async Task<ActionResult> UpdateEmployeeRecords([FromBody] UpdateEmployeeProfileRequestDto request)
        {
            var response = await _mediator.Send(request);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("getemployeeprojectsbyempNumber")]
        public async Task<IActionResult> GetEmployeeProjects(int employeeNumber)
        {
            var response = await _mediator.Send(new EmployeeProjectRequestDto { EmployeeNumber = employeeNumber });
            if (response == null) return NotFound($"Project not found for EmployeeNumber {employeeNumber}");
            return Ok(response);
        }

        [HttpGet("getemployeedesignationbyempNumber")]
        public async Task<IActionResult> GetEmployeeDesignation(int employeeNumber)
        {
            var response = await _mediator.Send(new EmployeeDesignationRequestDto { EmployeeNumber = employeeNumber });
            if (response == null) return NotFound($"Designation not found for EmployeeNumber {employeeNumber}");
            return Ok(response);
        }

        // GET: api/EmployeeLeaves/{employeeNumber}
        [HttpGet("getEmployeeleaves/{employeeNumber}")]
        public async Task<IActionResult> GetEmployeeLeaves(int employeeNumber, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received request to fetch leaves for EmployeeNumber {EmployeeNumber}", employeeNumber);

            var requestDto = new GetEmployeeLeavesRequestDto
            {
                EmployeeNumber = employeeNumber
            };

            var response = await _mediator.Send(requestDto, cancellationToken);

            if (response.Leaves == null || !response.Leaves.Any())
            {
                return NotFound(new
                {
                    Message = response.Message ?? "No leave requests found for this employee."
                });
            }

            return Ok(response);
        }

        [HttpGet("getemployeebyid/{employeeId}")]
        public async Task<IActionResult> GetEmployeeById(Guid employeeId)
        {
            var result = await _mediator.Send(new EmployeeProfileByIdRequestDto { EmployeeId = employeeId });
            if (result == null)
                return NotFound($"Employee with ID {employeeId} not found");

            return Ok(result);
        }

        [HttpGet("team-members")]
        public async Task<IActionResult> GetEmployeesByManager([FromQuery] Guid managerId)
        {
            var result = await _mediator.Send(
                new GetEmployeesByManagerRequestDto
                {
                    ManagerId = managerId
                });

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("payroll/create")]
        public async Task<IActionResult> CreatePayrollRecords([FromBody] CreatePayrollRecordsRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "Invalid request payload." });
            }

            var response = await _mediator.Send(request);
            if (!response.Success)
            {
                _logger.LogWarning($"Failed to create payroll record: {response.Message}");
                return BadRequest(response);
            }

            _logger.LogInformation($"Payroll record created successfully");

            return Ok(response);
        }

        [Authorize]
        [HttpGet("payroll/{employeeNumber}")]
        public async Task<IActionResult> GetPayrollRecords(int employeeNumber)
        {
            var request = new GetPayrollRecordsRequestDto { EmployeeNumber = employeeNumber };
            var response = await _mediator.Send(request);

            if (!response.Success)
            {
                _logger.LogWarning($"Failed to fetch payroll records: {response.Message}");
                return NotFound(response);
            }

            _logger.LogInformation($"Payroll records retrieved for Employee {employeeNumber}");
            return Ok(response);
        }

        /// <summary>
        /// Generates payslip PDF with database calculations.
        /// Rate-limited via PayslipPolicy (5 req/min) to prevent CPU and memory exhaustion.
        /// </summary>
        [Authorize(Roles = "Admin,Employee")]
        [EnableRateLimiting("PayslipPolicy")]
        [HttpPost("generate-payslip")]
        public async Task<IActionResult> GeneratePayslip([FromBody] GeneratePayslipRequestDto request)
        {
            // ✏️ MOVED UP: null check now runs before request is used in the log
            if (request == null)
            {
                return BadRequest(new { Message = "Invalid request payload." });
            }

            _logger.LogInformation($"Received request to generate payslip for employee {request.EmployeeNumber} for {request.PayrollMonth}/{request.PayrollYear}");

            var response = await _mediator.Send(request);

            if (!response.Success)
            {
                _logger.LogWarning($"Failed to generate payslip: {response.Message}");
                return BadRequest(response);
            }

            _logger.LogInformation($"Payslip generated successfully for employee {request.EmployeeNumber}");

            // ✅ ADDED: find the saved PDF inside wwwroot
            var fullPath = Path.Combine(_env.WebRootPath, response.FilePath.TrimStart('/'));

            // ✅ ADDED: check that the file exists
            if (!System.IO.File.Exists(fullPath))
            {
                _logger.LogWarning("Payslip file not found at {Path}", fullPath);
                return NotFound(new { Message = "Payslip file not found on server." });
            }

            // ✅ ADDED: read the PDF bytes
            var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);

            // ✏️ CHANGED: was `return Ok(response);`
            // This makes the browser download the PDF to the user's PC
            return File(bytes, "application/pdf", Path.GetFileName(fullPath));
        }

        [HttpGet("notifications/{employeeNumber}")]
        public async Task<IActionResult> GetNotifications(int employeeNumber)
        {
            var employee = await _dbContext.Employees
                .FirstOrDefaultAsync(e => e.EmployeeNumber == employeeNumber);

            if (employee == null)
            {
                return NotFound(new { Message = $"Employee with number {employeeNumber} not found." });
            }

            var notifications = await _dbContext.Notifications
                .Where(n => n.EmployeeId == employee.EmployeeId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpDelete("notifications/{notificationId}")]
        public async Task<IActionResult> DeleteNotification(Guid notificationId)
        {
            var notification = await _dbContext.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                _dbContext.Notifications.Remove(notification);
                await _dbContext.SaveChangesAsync();
            }
            return Ok();
        }
    }
}
