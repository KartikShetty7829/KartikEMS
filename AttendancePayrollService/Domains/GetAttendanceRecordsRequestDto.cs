using MediatR;

namespace AttendancePayrollService.Domains
{
    public class GetAttendanceRecordsRequestDto : IRequest<GetAttendanceRecordsResponseDto>
    { 
        public int EmployeeNumber { get; set; } 
    }
}
