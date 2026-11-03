using MediatR;

namespace EmployeeServiceDepartmentService.domains
{
    public class GeneratePayslipRequestDto : IRequest<GeneratePayslipResponseDto>
    {
        public int EmployeeNumber { get; set; }
        public int PayrollMonth { get; set; }
        public int PayrollYear { get; set; }
    }
}
