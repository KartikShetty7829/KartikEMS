using AttendancePayrollService.Domains;
using AttendanceService.Domains;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AttendancePayrollService.Controllers
{ 
    /// <summary>
    /// Thin controller — only routes requests and returns responses (SRP).
    /// Business logic is handled by MediatR handlers.
    /// Depends on interfaces, not concrete classes (DIP).
    /// Protected with ASP.NET Core native rate limiting (AttendancePolicy).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("AttendancePolicy")]
    public class AttendanceController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(IMediator mediator, ILogger<AttendanceController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("checkinout")]
        public async Task<IActionResult> CheckInOut([FromBody] CheckInOutRequestDto request)
        {
            _logger.LogInformation("Received attendance request for employee number {EmployeeNumber}", request.EmployeeNumber);

            var response = await _mediator.Send(request);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        // ✅ GET: api/Attendance/{employeeNumber}
        [HttpGet("{employeeNumber}")]
        public async Task<IActionResult> GetAttendanceRecords(int employeeNumber)
        {
            _logger.LogInformation("API called to fetch attendance records for employee number {EmployeeNumber}", employeeNumber);

            var request = new GetAttendanceRecordsRequestDto
            {
                EmployeeNumber = employeeNumber
            };

            var response = await _mediator.Send(request);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        //[HttpGet("getpendingattendancerecords")]
        //public async Task<IActionResult> GetPendingAttendanceRecords(CancellationToken cancellationToken)
        //{
        //    _logger.LogInformation("API call: Fetching pending attendance records");
//
        //    var response = await _mediator.Send(new GetPendingAttendanceRecordsRequestDto(), cancellationToken);
//
        //    if (!response.Success)
        //        return NotFound(response);
//
        //    return Ok(response);
        //}

        [HttpGet("pending-records")]
        public async Task<IActionResult> GetPendingAttendanceRecords([FromQuery] int employeeNumber)
        {
            var result = await _mediator.Send(
            new GetPendingAttendanceRecordsRequestDto
            {
                EmployeeNumber = employeeNumber
            });

            return Ok(result);
        }

        // ✅ Endpoint: Approve/Reject attendance record
        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateAttendanceStatus([FromBody] UpdateAttendanceStatusRequestDto request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("API call: Updating attendance record {RecordId} with action {Action}", request.RecordId, request.Action);

            var response = await _mediator.Send(request, cancellationToken);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
