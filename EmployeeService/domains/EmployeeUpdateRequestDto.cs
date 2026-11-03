using MediatR;

namespace EmployeeServiceDepartmentService.domains
{
    public class EmployeeUpdateRequestDto :IRequest<EmployeeUpdateResponseDto>
    {
        public int EmployeeNumber { get; set; }
        public DateTime HireDate { get; set; }
    }
}
