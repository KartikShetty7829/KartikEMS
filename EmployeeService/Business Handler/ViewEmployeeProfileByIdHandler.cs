using EmployeeService.domains.EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Exceptions;
using EmployeeServiceDepartmentService.interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeServiceDepartmentService.Business_Handler
{
    public class ViewEmployeeProfileByIdHandler : IRequestHandler<EmployeeProfileByIdRequestDto, EmployeeProfileResponseDto>
        {
            private readonly IGenericRepository<EmpDepartmentDbContext, Employees> _employeeRepo;
            private readonly IGenericRepository<EmpDepartmentDbContext, EmployeeAddress> _addressRepo;
            private readonly ILogger<ViewEmployeeProfileByIdHandler> _logger;

            public ViewEmployeeProfileByIdHandler(
                IGenericRepository<EmpDepartmentDbContext, Employees> employeeRepo,
                IGenericRepository<EmpDepartmentDbContext, EmployeeAddress> addressRepo,
                ILogger<ViewEmployeeProfileByIdHandler> logger)
            {
                _employeeRepo = employeeRepo;
                _addressRepo = addressRepo;
                _logger = logger;
            }

            public async Task<EmployeeProfileResponseDto> Handle(EmployeeProfileByIdRequestDto request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("Fetching profile for EmployeeId {EmployeeId}", request.EmployeeId);

                    var employee = await _employeeRepo.Query()
                        .FirstOrDefaultAsync(e => e.EmployeeId == request.EmployeeId, cancellationToken);

                    if (employee == null)
                    {
                        _logger.LogWarning("Employee with EmployeeId {EmployeeId} not found", request.EmployeeId);
                        throw new NotFoundException($"Employee with EmployeeId '{request.EmployeeId}' not found.");
                    }

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
