using MediatR;

namespace EmployeeService.domains
{
    public class EmployeeDesignationRequestDto : IRequest<EmployeeDesignationResponseDto>
    {
        public int EmployeeNumber { get; set; }   // ✅ use EmployeeNumber
    }
}
