using MediatR;

namespace EmployeeServiceDepartmentService.domains
{
    public class AssignDesignationRequestDto : IRequest<AssignDesignationResponseDto>
    {
        public int EmployeeNumber { get; set; }    // From UI
        public required string DesignationName { get; set; }   // From dropdown
        public DateTime StartDate { get; set; }                      // From UI
        public DateTime? EndDate { get; set; }                       // Optional
    }
}
