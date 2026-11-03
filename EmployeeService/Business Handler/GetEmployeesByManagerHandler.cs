using System.Linq;
using EmployeeService.domains;
using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Business_Handler
{
    public class GetEmployeesByManagerHandler: IRequestHandler<GetEmployeesByManagerRequestDto,List<ReportingEmployeeDto>>
    {
        private readonly IGenericRepository<EmpDepartmentDbContext, Employees> _employeeRepo;

        public GetEmployeesByManagerHandler(
        IGenericRepository<EmpDepartmentDbContext, Employees> employeeRepo)
        {
            _employeeRepo = employeeRepo;
        }

        public async Task<List<ReportingEmployeeDto>> Handle(
        GetEmployeesByManagerRequestDto request,
        CancellationToken cancellationToken)
        {
            var employees = await _employeeRepo.Query()
            .Where(x => x.ManagerId == request.ManagerId)
            .Select(x => new ReportingEmployeeDto
            {
                EmployeeId = x.EmployeeId,
                EmployeeNumber = x.EmployeeNumber,
                FullName = x.FirstName + " " + x.LastName
            })
            .ToListAsync(cancellationToken);

            return employees;
        }
    }
}
