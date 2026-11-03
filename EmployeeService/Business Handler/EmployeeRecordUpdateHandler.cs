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
    public class EmployeeRecordUpdateHandler : IRequestHandler<EmployeeRecordUpdateRequestDto, EmployeeRecordUpdateResponseDto>
    {
        private readonly IGenericRepository<EmpDepartmentDbContext, Employees> _employeeRepo;
        private readonly ILogger<EmployeeRecordUpdateHandler> _logger;
        private readonly IUnitOfWork<EmpDepartmentDbContext> _unitOfWork;
        private readonly EmpDepartmentDbContext _dbContext;

        public EmployeeRecordUpdateHandler(
            IGenericRepository<EmpDepartmentDbContext, Employees> employeeRepo,
            ILogger<EmployeeRecordUpdateHandler> logger,
            IUnitOfWork<EmpDepartmentDbContext> unitOfWork,
            EmpDepartmentDbContext dbContext)
        {
            _employeeRepo = employeeRepo;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
        }

        public async Task<EmployeeRecordUpdateResponseDto> Handle(EmployeeRecordUpdateRequestDto request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting update for employee {EmployeeNumber}", request.EmployeeNumber);

                var employee = await _employeeRepo.Query()
                                                  .FirstOrDefaultAsync(e => e.EmployeeNumber == request.EmployeeNumber, cancellationToken);

                if (employee == null)
                {
                    _logger.LogWarning("Employee with Id {EmployeeNumber} not found", request.EmployeeNumber);
                    throw new NotFoundException($"Employee with Id '{request.EmployeeNumber}' not found.");
                }

                // Attach employee to context so we can mark specific properties
                var entry = _dbContext.Entry(employee);

                // Update only provided fields
                if (!string.IsNullOrEmpty(request.FirstName))
                {
                    employee.FirstName = request.FirstName;
                    entry.Property(e => e.FirstName).IsModified = true;
                }

                if (!string.IsNullOrEmpty(request.LastName))
                {
                    employee.LastName = request.LastName;
                    entry.Property(e => e.LastName).IsModified = true;
                }

                if (!string.IsNullOrEmpty(request.Email))
                {
                    employee.Email = request.Email;
                    entry.Property(e => e.Email).IsModified = true;
                }

                if (!string.IsNullOrEmpty(request.phonenumber))
                {
                    employee.Phone = request.phonenumber;
                    entry.Property(e => e.Phone).IsModified = true;
                }

                if (request.HireDate.HasValue)
                {
                    employee.HireDate = request.HireDate.Value;
                    entry.Property(e => e.HireDate).IsModified = true;
                }

                // Save changes
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Employee {EmployeeId} updated successfully", employee.EmployeeId);

                return new EmployeeRecordUpdateResponseDto
                {
                    Success = true,
                    Message = CommonMessages.EmployeeUpdate
                };
        }
    }
}
