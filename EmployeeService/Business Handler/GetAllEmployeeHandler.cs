using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeServiceDepartmentService.Business_Handler
{
    public class GetAllEmployeeHandler : IRequestHandler<GetAllEmployeeRequestDto, GetAllEmployeeResponseDto>
    {
        private readonly IGenericRepository<EmpDepartmentDbContext, Employees> _employeeRepo;
        private readonly ILogger<GetAllEmployeeHandler> _logger;

        public GetAllEmployeeHandler(
            IGenericRepository<EmpDepartmentDbContext, Employees> employeeRepo,
            ILogger<GetAllEmployeeHandler> logger)
        {
            _employeeRepo = employeeRepo;
            _logger = logger;
        }

        public async Task<GetAllEmployeeResponseDto> Handle(GetAllEmployeeRequestDto request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all employees excluding UpdateValue = 0");

            var employees = await _employeeRepo.Query()
                        .Where(e => e.Status) // only true values
                        .ToListAsync(cancellationToken);

            // Manual mapping instead of AutoMapper
            var employeeDtos = employees.Select(e => new EmployeeDto
            {
                EmployeeId = e.EmployeeId,
                EmployeeNumber = e.EmployeeNumber,
                FullName = e.FirstName + " " + e.LastName,
                Email = e.Email,
                Phone = e.Phone,
                HireDate = e.HireDate,
                Status = e.Status ? "Active" : "Inactive"
            }).ToList();


            return new GetAllEmployeeResponseDto
            {
                Employees = employeeDtos
            };
    }
    }
}
