using MediatR;

namespace AttendanceService.Domains
{
    public class GetPendingAttendanceRecordsRequestDto : IRequest<GetPendingAttendanceRecordsResponseDto>
    {
        

        public int EmployeeNumber { get; set; }
      
    }
}
