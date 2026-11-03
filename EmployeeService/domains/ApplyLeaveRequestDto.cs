using MediatR;

namespace EmployeeServiceDepartmentService.domains
{
    public class ApplyLeaveRequestDto : IRequest<ApplyLeaveResponseDto>
    {
        public int EmployeeNumber { get; set; }
        public string Type { get; set; } 
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
