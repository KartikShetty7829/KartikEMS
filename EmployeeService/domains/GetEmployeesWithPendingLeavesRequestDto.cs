using MediatR;

namespace EmployeeServiceDepartmentService.domains
{
    public class GetEmployeesWithPendingLeavesRequestDto : IRequest<GetEmployeesWithPendingLeavesResponseDto>
    {
  
        public int EmployeeNumber { get; set; } 
        // No parameters needed, but you could add filters (e.g., date range) later
    }
}
