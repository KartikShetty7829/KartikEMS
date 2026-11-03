using MediatR;

namespace EmployeeServiceDepartmentService.domains
{
    public class GetEmployeeLeavesRequestDto : IRequest<GetEmployeeLeavesResponseDto>
    {
        public int EmployeeNumber { get; set; }
    }

  
}
