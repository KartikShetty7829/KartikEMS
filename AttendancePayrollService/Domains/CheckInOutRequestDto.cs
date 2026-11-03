using MediatR;

namespace AttendancePayrollService.Domains
{
    public class CheckInOutRequestDto : IRequest<CheckInOutResponseDto>
    {
        public int EmployeeNumber { get; set; }   // Input from client
        public DateTime CheckInAt {  get; set; }
        public DateTime CheckOutAt { get; set; }
    }
}
