using AttendancePayrollService.DataAccess;
using AttendancePayrollService.Domains;
using AttendancePayrollService.Entity;
using AttendancePayrollService.Exceptions;
using AttendancePayrollService.ExternalService;
using AttendancePayrollService.Interfaces;

using AttendanceService.Businesshandler;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AttendancePayrollService.Businesshandler
{
    public class CheckInOutHandler : IRequestHandler<CheckInOutRequestDto, CheckInOutResponseDto>
    {
        private readonly IGenericRepository<EmpAttendanceDbContext, AttendanceRecord> _attendanceRepo;
        private readonly ILogger<CheckInOutHandler> _logger;
        private readonly IUnitOfWork<EmpAttendanceDbContext> _unitOfWork;
        private readonly IEmployeeServiceClient _employeeServiceClient;
    

        public CheckInOutHandler(
            IGenericRepository<EmpAttendanceDbContext, AttendanceRecord> attendanceRepo,
            ILogger<CheckInOutHandler> logger,
            IUnitOfWork<EmpAttendanceDbContext> unitOfWork,
            IEmployeeServiceClient employeeServiceClient
            )
        {
            _attendanceRepo = attendanceRepo;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _employeeServiceClient = employeeServiceClient;
            
        }

        public async Task<CheckInOutResponseDto> Handle(CheckInOutRequestDto request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing attendance for employee number {EmployeeNumber}", request.EmployeeNumber);

            try
            {
                var employeeId = await _employeeServiceClient.GetEmployeeIdByNumberAsync(request.EmployeeNumber);
                if (employeeId == null)
                {
                    throw new NotFoundException("Employee not found.");
                }

                var existingAttendance = await _attendanceRepo
                    .GetAttendanceRecord()
                    .FirstOrDefaultAsync(a =>
                        a.EmployeeId == employeeId.Value &&
                        a.CheckInAt.Date == request.CheckInAt.Date);

                if (existingAttendance != null)
                {
                    throw new BadRequestException("Attendance already recorded for today.");
                }

                var attendance = new AttendanceRecord
                {
                    RecordId = Guid.NewGuid(),
                    EmployeeId = employeeId.Value,
                    CheckInAt = request.CheckInAt,
                    CheckOutAt = request.CheckOutAt,
                    Status = "Pending"
                };

                await _attendanceRepo.AddAsync(attendance);
                await _unitOfWork.SaveChangesAsync();

                // 👇 Fetch employee details for publishing
                

                return new CheckInOutResponseDto { Success = true, Message = "Applied successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing attendance for employee number {EmployeeNumber}", request.EmployeeNumber);
                throw;
            }
        }

    }
}

