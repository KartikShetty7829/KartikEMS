using AttendancePayrollService.DataAccess;
using AttendancePayrollService.Entity;
using AttendancePayrollService.Exceptions;
using AttendancePayrollService.ExternalService;
using AttendancePayrollService.Interfaces;
using AttendanceService.Domains;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AttendanceService.Businesshandler
{
    public class UpdateAttendanceStatusHandler
        : IRequestHandler<UpdateAttendanceStatusRequestDto, UpdateAttendanceStatusResponseDto>
    {
        private readonly IGenericRepository<EmpAttendanceDbContext, AttendanceRecord> _attendanceRepo;
        private readonly IUnitOfWork<EmpAttendanceDbContext> _unitOfWork;
        private readonly ILogger<UpdateAttendanceStatusHandler> _logger;
        private readonly IEmployeeServiceClient _employeeServiceClient;

        public UpdateAttendanceStatusHandler(
            IGenericRepository<EmpAttendanceDbContext, AttendanceRecord> attendanceRepo,
            IUnitOfWork<EmpAttendanceDbContext> unitOfWork,
            ILogger<UpdateAttendanceStatusHandler> logger,
            IEmployeeServiceClient employeeServiceClient)
        {
            _attendanceRepo = attendanceRepo;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _employeeServiceClient = employeeServiceClient;
        }

        public async Task<UpdateAttendanceStatusResponseDto> Handle(
            UpdateAttendanceStatusRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                // 1. Find attendance record
                var attendanceRecord = await _attendanceRepo
                    .GetAttendanceRecord()
                    .FirstOrDefaultAsync(
                        a => a.RecordId == request.RecordId,
                        cancellationToken);

                if (attendanceRecord == null)
                {
                    throw new NotFoundException(
                        $"Attendance record with ID {request.RecordId} not found");
                }

                // 2. Validate action
                var action = request.Action.Trim().ToLower();

                if (action != "approve" && action != "reject")
                {
                    throw new BadRequestException(
                        "Invalid action. Must be 'Approve' or 'Reject'.");
                }

                // 3. Get Approver EmployeeId
                var approverId = await _employeeServiceClient
                    .GetEmployeeIdByNumberAsync(request.EmployeeNumber);

                if (approverId == null)
                {
                    throw new NotFoundException(
                        $"Employee not found for Employee Number {request.EmployeeNumber}");
                }

                // 4. Update Attendance Record
                attendanceRecord.Status =
                    action == "approve" ? "Approved" : "Rejected";

                attendanceRecord.ApprovedBy = request.AdminUserName;

                attendanceRecord.ApproverId = approverId.Value;

                attendanceRecord.ApprovedAt = DateTime.UtcNow;

                await _attendanceRepo.UpdateAsync(attendanceRecord);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Attendance Record {RecordId} updated successfully by {AdminUserName}",
                    request.RecordId,
                    request.AdminUserName);

                return new UpdateAttendanceStatusResponseDto
                {
                    Success = true,
                    Message = $"Attendance record {attendanceRecord.Status} successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating attendance record {RecordId}",
                    request.RecordId);

                throw;
            }
        }
    }
}