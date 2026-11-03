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
    public class UpdateEmployeeProfileHandler : IRequestHandler<UpdateEmployeeProfileRequestDto, UpdateEmployeeProfileResponseDto>
    {
        private readonly IGenericRepository<EmpDepartmentDbContext, Employees> _employeeRepo;
        private readonly IGenericRepository<EmpDepartmentDbContext, EmployeeAddress> _addressRepo;
        private readonly ILogger<UpdateEmployeeProfileHandler> _logger;
        private readonly IUnitOfWork<EmpDepartmentDbContext>? _unitOfWork;

        public UpdateEmployeeProfileHandler(
            IGenericRepository<EmpDepartmentDbContext, Employees> employeeRepo,
            IGenericRepository<EmpDepartmentDbContext, EmployeeAddress> addressRepo,
            ILogger<UpdateEmployeeProfileHandler> logger,
            IUnitOfWork<EmpDepartmentDbContext>? unitOfWork = null)
        {
            _employeeRepo = employeeRepo;
            _addressRepo = addressRepo;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<UpdateEmployeeProfileResponseDto> Handle(UpdateEmployeeProfileRequestDto request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating profile for EmployeeNumber {EmployeeNumber}", request.EmployeeNumber);

                var employee = await _employeeRepo.Query()
                    .FirstOrDefaultAsync(e => e.EmployeeNumber == request.EmployeeNumber, cancellationToken);

                if (employee == null)
                {
                    _logger.LogWarning("Employee with EmployeeNumber {EmployeeNumber} not found", request.EmployeeNumber);
                    throw new NotFoundException("Employee not found");
                }

                // Update employee fields if provided (EmployeeNumber excluded)
                if (!string.IsNullOrEmpty(request.Email)) employee.Email = request.Email;
                if (!string.IsNullOrEmpty(request.Phone)) employee.Phone = request.Phone;

                // Do not update EmployeeNumber — it's identity
                await _employeeRepo.UpdateAsync(employee);


                // Update or add address
                var existingAddress = await _addressRepo.Query()
                    .FirstOrDefaultAsync(a => a.EmployeeId == employee.EmployeeId, cancellationToken);

                if (existingAddress != null)
                {
                    existingAddress.AddressLine1 = request.AddressLine1;
                    existingAddress.AddressLine2 = request.AddressLine2;
                    existingAddress.City = request.City;
                    existingAddress.State = request.State;
                    existingAddress.PostalCode = request.PostalCode;
                    existingAddress.Country = request.Country;
                    existingAddress.AddressType = request.AddressType;

                    await _addressRepo.UpdateAsync(existingAddress);
                }
                else
                {
                    var newAddress = new EmployeeAddress
                    {
                        EmployeeId = employee.EmployeeId,
                        AddressLine1 = request.AddressLine1,
                        AddressLine2 = request.AddressLine2,
                        City = request.City,
                        State = request.State,
                        PostalCode = request.PostalCode,
                        Country = request.Country,
                        AddressType = request.AddressType
                    };

                    await _addressRepo.AddAsync(newAddress);
                }

                if (_unitOfWork != null)
                {
                    await _unitOfWork.SaveChangesAsync();
                }

                return new UpdateEmployeeProfileResponseDto
                {
                    Success = true,
                    Message = "Employee data updated successfully"
                };
        }
    }
}
