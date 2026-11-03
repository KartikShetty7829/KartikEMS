using MediatR;

namespace EmployeeServiceDepartmentService.domains
{
    public class CreatePayrollRecordsRequestDto : IRequest<CreatePayrollRecordsResponseDto>
    {
        public int EmployeeNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
