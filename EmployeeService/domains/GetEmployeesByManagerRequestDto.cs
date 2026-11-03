using MediatR;

namespace EmployeeService.domains
{
    public class GetEmployeesByManagerRequestDto: IRequest<List<ReportingEmployeeDto>>
    {
        public Guid ManagerId { get; set; }
    }
}
