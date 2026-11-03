using AttendancePayrollService.DataAccess;
using AttendancePayrollService.Entity;
using AttendancePayrollService.ExternalService;
using AttendancePayrollService.externalservicereponse;
using AttendancePayrollService.Interfaces;
using AttendanceService.Domains;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AttendanceService.Businesshandler
{
    public class GetPendingAttendanceRecordsHandler
        : IRequestHandler<GetPendingAttendanceRecordsRequestDto,
            GetPendingAttendanceRecordsResponseDto>
    {
        private readonly IGenericRepository<EmpAttendanceDbContext, AttendanceRecord> _attendanceRepo;
        private readonly ILogger<GetPendingAttendanceRecordsHandler> _logger;
        private readonly IEmployeeServiceClient _employeeServiceClient;

        public GetPendingAttendanceRecordsHandler(
            IGenericRepository<EmpAttendanceDbContext, AttendanceRecord> attendanceRepo,
            ILogger<GetPendingAttendanceRecordsHandler> logger,
            IEmployeeServiceClient employeeServiceClient)
        {
            _attendanceRepo = attendanceRepo;
            _logger = logger;
            _employeeServiceClient = employeeServiceClient;
        }

        public async Task<GetPendingAttendanceRecordsResponseDto> Handle(
            GetPendingAttendanceRecordsRequestDto request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Fetching pending attendance records for Manager Employee Number {EmployeeNumber}",
                request.EmployeeNumber);

            try
            {
                // Step 1 : Get Manager EmployeeId using EmployeeNumber
                var managerId =
                    await _employeeServiceClient.GetEmployeeIdByNumberAsync(
                        request.EmployeeNumber);

                if (managerId == null)
                {
                    return new GetPendingAttendanceRecordsResponseDto
                    {
                        Success = false,
                        Message = "Manager not found."
                    };
                }

                // Step 2 : Get all employees reporting to this manager
                var teamMembers =
                    await _employeeServiceClient.GetEmployeesByManagerIdAsync(
                        managerId.Value);

                if (teamMembers == null || !teamMembers.Any())
                {
                    return new GetPendingAttendanceRecordsResponseDto
                    {
                        Success = false,
                        Message = "No team members found."
                    };
                }

                // Step 3 : Extract EmployeeIds
                var employeeIds = teamMembers
                    .Select(x => x.EmployeeId)
                    .ToList();

                // Step 4 : Get Pending Attendance Records
                var records = await _attendanceRepo
                    .GetAttendanceRecord()
                    .Where(a =>
                        employeeIds.Contains(a.EmployeeId) &&
                        a.Status == "Pending")
                    .OrderByDescending(a => a.CheckInAt)
                    .ToListAsync(cancellationToken);

                if (!records.Any())
                {
                    return new GetPendingAttendanceRecordsResponseDto
                    {
                        Success = false,
                        Message = "No pending attendance records found."
                    };
                }

                // Step 5 : Map response
                var enrichedRecords = records
                    .Select(record =>
                    {
                        var employee = teamMembers
                            .FirstOrDefault(x =>
                                x.EmployeeId == record.EmployeeId);

                        return new AttendanceRecordDto
                        {
                            RecordId = record.RecordId,
                            FullName = employee?.FullName ?? "Unknown",
                            CheckInAt = record.CheckInAt,
                            CheckOutAt = record.CheckOutAt,
                            Status = record.Status
                        };
                    })
                    .ToList();

                return new GetPendingAttendanceRecordsResponseDto
                {
                    Success = true,
                    Message = "Pending attendance records retrieved successfully.",
                    Records = enrichedRecords
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while fetching pending attendance records");

                throw;
            }
        }
    }
}