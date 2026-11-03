using EmployeeServiceDepartmentService.Common;
using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Exceptions;
using EmployeeServiceDepartmentService.interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeServiceDepartmentService.Business_Handler
{
    public class EmployeeRecordDeleteHandler : IRequestHandler<EmployeeRecordDeleteRequestDto, EmployeeRecordDeleteResponseDto>
    {
        private readonly IGenericRepository<EmpDepartmentDbContext, Employees> _employeeRepo;
        private readonly ILogger<EmployeeRecordDeleteHandler> _logger;
        private readonly IUnitOfWork<EmpDepartmentDbContext> _unitOfWork;
        private readonly EmpDepartmentDbContext _dbContext;

        public EmployeeRecordDeleteHandler(
            IGenericRepository<EmpDepartmentDbContext, Employees> employeeRepo,
            ILogger<EmployeeRecordDeleteHandler> logger,
            IUnitOfWork<EmpDepartmentDbContext> unitOfWork,
            EmpDepartmentDbContext dbContext)
        {
            _employeeRepo = employeeRepo;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
        }

        public async Task<EmployeeRecordDeleteResponseDto> Handle(EmployeeRecordDeleteRequestDto request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating status for employee {EmployeeNumber}", request.EmployeeNumber);

                var employee = await _employeeRepo.Query()
                                                  .FirstOrDefaultAsync(e => e.EmployeeNumber == request.EmployeeNumber, cancellationToken);

                if (employee == null)
                {
                    _logger.LogWarning("Employee with Id {EmployeeNumber} not found", request.EmployeeNumber);
                    throw new NotFoundException($"Employee with Id '{request.EmployeeNumber}' not found.");
                }

                // Update status using enum
                employee.Status = request.Status == EmployeeStatus.Active;

                var entry = _dbContext.Entry(employee);
                entry.Property(e => e.Status).IsModified = true;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Employee {EmployeeId} status updated to {Status}", employee.EmployeeId, request.Status);

                return new EmployeeRecordDeleteResponseDto
                {
                    Success = true,
                    Message = request.Status == EmployeeStatus.Inactive
                        ? CommonMessages.EmployeeMarkedInactive
                        : CommonMessages.EmployeeMarkedActive
                };
        }
    }
}
