using MediatR;

namespace EmployeeServiceDepartmentService.domains
{
    public class DefinePayrollRequestDto : IRequest<DefinePayrollResponseDto>
    {
        public required int EmployeeNumber { get; set; }// Employee reference
        public int PayrollMonth { get; set; }         // e.g., 1 = Jan
        public int PayrollYear { get; set; }
        public decimal BaseSalary { get; set; }// e.g., 2025
        public decimal Allowances { get; set; }       // New allowances
        public decimal Deductions { get; set; }       // New deductions
    }

}
