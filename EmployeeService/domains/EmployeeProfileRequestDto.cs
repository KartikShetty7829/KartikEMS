using MediatR;

namespace EmployeeServiceDepartmentService.domains
{
    public class EmployeeProfileRequestDto : IRequest<EmployeeProfileResponseDto>
    {
        public int EmployeeNumber { get; set; }
    }
}
