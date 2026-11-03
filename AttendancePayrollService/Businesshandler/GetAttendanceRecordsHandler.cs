using AttendancePayrollService.DataAccess;
using AttendancePayrollService.Domains;
using AttendancePayrollService.Entity;
using AttendancePayrollService.Exceptions;
using AttendancePayrollService.ExternalService;
using AttendancePayrollService.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AttendancePayrollService.Businesshandler
{
    public class GetAttendanceRecordsHandler : IRequestHandler<GetAttendanceRecordsRequestDto, GetAttendanceRecordsResponseDto>
    {
        private readonly IGenericRepository<EmpAttendanceDbContext, AttendanceRecord> _attendanceRepo;
        private readonly ILogger<GetAttendanceRecordsHandler> _logger;
        private readonly IEmployeeServiceClient _employeeServiceClient;

        public GetAttendanceRecordsHandler(
            IGenericRepository<EmpAttendanceDbContext, AttendanceRecord> attendanceRepo,
            ILogger<GetAttendanceRecordsHandler> logger,
            IEmployeeServiceClient employeeServiceClient)
        {
            _attendanceRepo = attendanceRepo;
            _logger = logger;
            _employeeServiceClient = employeeServiceClient;
        }

        public async Task<GetAttendanceRecordsResponseDto> Handle(GetAttendanceRecordsRequestDto request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching attendance records for employee number {EmployeeNumber}", request.EmployeeNumber);

            try
            {
                // 1. Resolve EmployeeId from external service
                var employeeId = await _employeeServiceClient.GetEmployeeIdByNumberAsync(request.EmployeeNumber);
                if (employeeId == null)
                {
                    _logger.LogWarning("Employee number {EmployeeNumber} not found", request.EmployeeNumber);
                    throw new NotFoundException("Employee not found.");
                }

                // 2. Fetch records from DB
                var records = await _attendanceRepo
                    .GetAttendanceRecord()
                    .Where(a => a.EmployeeId == employeeId.Value)
                    .OrderByDescending(a => a.CheckInAt)
                    .Select(a => new AttendanceRecordDto
                    {
                        CheckInAt = a.CheckInAt,
                        CheckOutAt = a.CheckOutAt,
                        ApprovedBy=a.ApprovedBy,
                        ApprovedAt=a.ApprovedAt,
                        Status=a.Status
                    })
                    .ToListAsync(cancellationToken);

                if (!records.Any())
                {
                    return new GetAttendanceRecordsResponseDto
                    {
                        Success = false,
                        Message = "No attendance records found."
                    };
                }

                return new GetAttendanceRecordsResponseDto
                {
                    Success = true,
                    Message = "Attendance records retrieved successfully.",
                    Records = records
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching attendance for employee number {EmployeeNumber}", request.EmployeeNumber);
                throw;
            }
        }
    }
}
