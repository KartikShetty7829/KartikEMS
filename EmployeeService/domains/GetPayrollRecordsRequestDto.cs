using MediatR;

namespace EmployeeServiceDepartmentService.domains
{
    public class GetPayrollRecordsRequestDto : IRequest<GetPayrollRecordsResponseDto>
    {
        public int EmployeeNumber { get; set; }
    }
}
