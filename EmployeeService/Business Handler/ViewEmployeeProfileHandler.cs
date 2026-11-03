using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Exceptions;
using EmployeeServiceDepartmentService.interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeServiceDepartmentService.Business_Handler
{
    public class ViewEmployeeProfileHandler : IRequestHandler<EmployeeProfileRequestDto, EmployeeProfileResponseDto>
    {
        private readonly IGenericRepository<EmpDepartmentDbContext, Employees> _employeeRepo;
        private readonly IGenericRepository<EmpDepartmentDbContext, EmployeeAddress> _addressRepo;
        private readonly ILogger<ViewEmployeeProfileHandler> _logger;
       

        public ViewEmployeeProfileHandler(
            IGenericRepository<EmpDepartmentDbContext, Employees> employeeRepo,
            IGenericRepository<EmpDepartmentDbContext, EmployeeAddress> addressRepo,
            ILogger<ViewEmployeeProfileHandler> logger
           )
        {
            _employeeRepo = employeeRepo;
            _addressRepo = addressRepo;
            _logger = logger;
           
        }

        public async Task<EmployeeProfileResponseDto> Handle(EmployeeProfileRequestDto request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching profile for EmployeeNumber {EmployeeNumber}", request.EmployeeNumber);

            var employee = await _employeeRepo.Query()
                    .FirstOrDefaultAsync(e => e.EmployeeNumber == request.EmployeeNumber, cancellationToken);

                if (employee == null)
                {
                    _logger.LogWarning("Employee with EmployeeNumber {EmployeeNumber} not found", request.EmployeeNumber);
                    throw new NotFoundException($"Employee with EmployeeNumber '{request.EmployeeNumber}' not found.");
                }

                // Fetch addresses for this employee
                var existingAddresses = await _addressRepo
                    .Query()
                    .Where(a => a.EmployeeId == employee.EmployeeId)
                    .ToListAsync(cancellationToken);

                var primaryAddress = existingAddresses.FirstOrDefault();

              

                return new EmployeeProfileResponseDto
                {
                    EmployeeId = employee.EmployeeId,
                    EmployeeNumber = employee.EmployeeNumber,
                    FullName = $"{employee.FirstName} {employee.LastName}",
                    Email = employee.Email,
                    Phone = employee.Phone,
                    HireDate = employee.HireDate,
                    Status = employee.Status,

                    // Return SAS URL if generated, otherwise fallback to raw URL
                 

                    AddressLine1 = primaryAddress?.AddressLine1 ?? string.Empty,
                    AddressLine2 = primaryAddress?.AddressLine2,
                    City = primaryAddress?.City ?? string.Empty,
                    State = primaryAddress?.State ?? string.Empty,
                    PostalCode = primaryAddress?.PostalCode ?? string.Empty,
                    Country = primaryAddress?.Country ?? string.Empty,
                    AddressType = primaryAddress?.AddressType ?? string.Empty
                };
        }
    }
}
