using MediatR;

namespace EmployeeServiceDepartmentService.domains
{
    public class EmployeeRequestDto : IRequest<EmployeeResponseDto>
    {
 
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public DateTime HireDate { get; set; }
        public required bool Status { get; set; } 
    }
}
