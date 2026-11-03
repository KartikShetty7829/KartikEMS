using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Exceptions;
using EmployeeServiceDepartmentService.interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeServiceDepartmentService.Business_Handler
{
    public class UpdateEmployeeHireDateHandler : IRequestHandler<EmployeeUpdateRequestDto, EmployeeUpdateResponseDto>
    {
        private readonly IGenericRepository<EmpDepartmentDbContext, Employees> _employeeRepo;
        private readonly ILogger<UpdateEmployeeHireDateHandler> _logger;
        private readonly IUnitOfWork<EmpDepartmentDbContext> _unitOfWork;

        public UpdateEmployeeHireDateHandler(
            IGenericRepository<EmpDepartmentDbContext, Employees> employeeRepo,
            ILogger<UpdateEmployeeHireDateHandler> logger,
            IUnitOfWork<EmpDepartmentDbContext> unitOfWork)
        {
            _employeeRepo = employeeRepo;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<EmployeeUpdateResponseDto> Handle(EmployeeUpdateRequestDto request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting update of HireDate for employee {EmployeeId}", request.EmployeeNumber);

                // 1. Find employee by EmployeeNumber
                var employee = await _employeeRepo.Query()
                                                  .FirstOrDefaultAsync(e => e.EmployeeNumber == request.EmployeeNumber, cancellationToken);

                if (employee == null)
                {
                    _logger.LogWarning("Employee with Id {EmployeeId} not found", request.EmployeeNumber);
                    throw new NotFoundException($"Employee with Id '{request.EmployeeNumber}' not found.");
                }

                // 2. Update only HireDate
                employee.HireDate = request.HireDate;

                await _employeeRepo.UpdateAsync(employee);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Employee {EmployeeId} HireDate updated to {HireDate}", employee.EmployeeId, employee.HireDate);

                // 3. Build response
                return new EmployeeUpdateResponseDto
                {
                    Success = true,
                    Message = $"Employee '{employee.FirstName} {employee.LastName}' HireDate updated successfully."
                };
        }
    }
}
