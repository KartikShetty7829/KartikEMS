using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Exceptions;
using EmployeeServiceDepartmentService.interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeServiceDepartmentService.Business_Handler
{
    public class AddEmployeeAddressHandler
        : IRequestHandler<AddEmployeeAddressesRequestDto, AddEmployeeAddressesResponseDto>
    {
        private readonly IGenericRepository<EmpDepartmentDbContext, Employees> _employeeRepo;
        private readonly IGenericRepository<EmpDepartmentDbContext, EmployeeAddress> _addressRepo;
        private readonly IUnitOfWork<EmpDepartmentDbContext> _unitOfWork;
        private readonly ILogger<AddEmployeeAddressHandler> _logger;

        public AddEmployeeAddressHandler(
            IGenericRepository<EmpDepartmentDbContext, Employees> employeeRepo,
            IGenericRepository<EmpDepartmentDbContext, EmployeeAddress> addressRepo,
            IUnitOfWork<EmpDepartmentDbContext> unitOfWork,
            ILogger<AddEmployeeAddressHandler> logger)
        {
            _employeeRepo = employeeRepo;
            _addressRepo = addressRepo;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<AddEmployeeAddressesResponseDto> Handle(AddEmployeeAddressesRequestDto request, CancellationToken cancellationToken)
        {
            foreach (var empDto in request.Employees)
            {
                    // 1. Lookup Employee by EmployeeNumber
                    var employee = await _employeeRepo
                        .Query()
                        .FirstOrDefaultAsync(e => e.EmployeeNumber == empDto.EmployeeNumber, cancellationToken);

                    if (employee == null)
                    {
                        _logger.LogWarning("Employee with number {EmployeeNumber} not found", empDto.EmployeeNumber);
                        continue; // Skip this employee, move to next
                    }

                    // 2. Check if employee already has addresses
                    var existingAddresses = await _addressRepo
                        .Query()
                        .Where(a => a.EmployeeId == employee.EmployeeId)
                        .ToListAsync(cancellationToken);

                    if (existingAddresses.Any())
                    {
                        _logger.LogInformation("Employee {EmployeeNumber} already has addresses. Skipping insert.", empDto.EmployeeNumber);
                        continue;
                    }

                    // 3. Add all addresses for this employee
                    foreach (var dto in empDto.Addresses)
                    {
                        var addressEntity = new EmployeeAddress
                        {
                            EmployeeId = employee.EmployeeId,
                            AddressLine1 = dto.AddressLine1,
                            AddressLine2 = dto.AddressLine2,
                            City = dto.City,
                            State = dto.State,
                            PostalCode = dto.PostalCode,
                            Country = dto.Country,
                            AddressType = dto.AddressType
                        };

                    await _addressRepo.AddAsync(addressEntity);
                }
            }

            // 4. Save all changes once
            await _unitOfWork.SaveChangesAsync();

            return new AddEmployeeAddressesResponseDto
            {
                Success = true,
                Message = "Addresses saved successfully"
            };
        }
    }
}
