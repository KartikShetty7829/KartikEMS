using MediatR;

namespace EmployeeService.domains
{
    public class EmployeeProjectRequestDto : IRequest<EmployeeProjectResponseDto>
    {
        public int EmployeeNumber { get; set; }  // ✅ use EmployeeNumber
    }
}
