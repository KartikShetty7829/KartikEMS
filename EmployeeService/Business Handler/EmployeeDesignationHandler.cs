using EmployeeService.domains;
using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Exceptions;
using EmployeeServiceDepartmentService.interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Business_Handler
{
    public class EmployeeDesignationHandler : IRequestHandler<EmployeeDesignationRequestDto, EmployeeDesignationResponseDto>
    {
        private readonly IGenericRepository<EmpDepartmentDbContext, EmployeeDesignation> _employeeDesignationRepo;
        private readonly ILogger<EmployeeDesignationHandler> _logger;

        public EmployeeDesignationHandler(
            IGenericRepository<EmpDepartmentDbContext, EmployeeDesignation> employeeDesignationRepo,
            ILogger<EmployeeDesignationHandler> logger)
        {
            _employeeDesignationRepo = employeeDesignationRepo;
            _logger = logger;
        }

        public async Task<EmployeeDesignationResponseDto> Handle(EmployeeDesignationRequestDto request, CancellationToken cancellationToken)
        {
            var designation = await _employeeDesignationRepo
                .Query()
                .Include(ed => ed.Designation)   // eager load Designation
                .Include(ed => ed.Employee)      // eager load Employee
                .FirstOrDefaultAsync(ed => ed.Employee.EmployeeNumber == request.EmployeeNumber, cancellationToken);

            if (designation == null)
                throw new NotFoundException($"Designation not found for employee number {request.EmployeeNumber}");

            return new EmployeeDesignationResponseDto
            {
                EmployeeNumber = request.EmployeeNumber,
                DesignationName = designation.Designation.Title,
                StartDate = designation.StartDate,
                EndDate = designation.EndDate
            };
        }

    }
}
