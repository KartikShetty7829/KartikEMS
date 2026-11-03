using EmployeeService.domains;
using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Business_Handler
{
    public class EmployeeProjectHandler : IRequestHandler<EmployeeProjectRequestDto, EmployeeProjectResponseDto>
    {
        private readonly IGenericRepository<EmpDepartmentDbContext, EmployeeProject> _employeeProjectRepo;
        private readonly ILogger<EmployeeProjectHandler> _logger;

        public EmployeeProjectHandler(
            IGenericRepository<EmpDepartmentDbContext, EmployeeProject> employeeProjectRepo,
            ILogger<EmployeeProjectHandler> logger)
        {
            _employeeProjectRepo = employeeProjectRepo;
            _logger = logger;
        }

        public async Task<EmployeeProjectResponseDto> Handle(EmployeeProjectRequestDto request, CancellationToken cancellationToken)
        {
            var projects = await _employeeProjectRepo
                .Query()
                .Include(ep => ep.Project)     // eager load Project
                .Include(ep => ep.Employee)    // eager load Employee
                .Where(ep => ep.Employee.EmployeeNumber == request.EmployeeNumber)
                .Select(ep => new ProjectDto
                {
                    Name = ep.Project.Name,
                    Description = ep.Project.Description,
                    StartDate = ep.Project.StartDate,
                    EndDate = ep.Project.EndDate,
                    Status = ep.Project.Status
                })
                .ToListAsync(cancellationToken);

            return new EmployeeProjectResponseDto
            {
                EmployeeNumber = request.EmployeeNumber,
                Projects = projects
            };
        }
    }

}
