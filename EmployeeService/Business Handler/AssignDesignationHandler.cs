using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Exceptions;
using EmployeeServiceDepartmentService.interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeServiceDepartmentService.Business_Handler
{
    public class AssignDesignationHandler
        : IRequestHandler<AssignDesignationRequestDto, AssignDesignationResponseDto>
    {
        private readonly IGenericRepository<EmpDepartmentDbContext, Employees> _employeeRepo;
        private readonly IGenericRepository<EmpDepartmentDbContext, Designation> _designationRepo;
        private readonly IGenericRepository<EmpDepartmentDbContext, EmployeeDesignation> _employeeDesignationRepo;
        private readonly IUnitOfWork<EmpDepartmentDbContext> _unitOfWork;
        private readonly ILogger<AssignDesignationHandler> _logger;

        public AssignDesignationHandler(
            IGenericRepository<EmpDepartmentDbContext, Employees> employeeRepo,
            IGenericRepository<EmpDepartmentDbContext, Designation> designationRepo,
            IGenericRepository<EmpDepartmentDbContext, EmployeeDesignation> employeeDesignationRepo,
            IUnitOfWork<EmpDepartmentDbContext> unitOfWork,
            ILogger<AssignDesignationHandler> logger)
        {
            _employeeRepo = employeeRepo;
            _designationRepo = designationRepo;
            _employeeDesignationRepo = employeeDesignationRepo;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<AssignDesignationResponseDto> Handle(AssignDesignationRequestDto request, CancellationToken cancellationToken)
        {
                // 1. Lookup Employee by EmployeeNumber
                var employee = await _employeeRepo
                    .Query()
                    .FirstOrDefaultAsync(e => e.EmployeeNumber == request.EmployeeNumber, cancellationToken);

                if (employee == null)
                {
                    throw new NotFoundException($"Employee with number {request.EmployeeNumber} not found");
                }

                // 2. Lookup Designation by Name
                var designation = await _designationRepo
                    .Query()
                    .FirstOrDefaultAsync(d => d.Title == request.DesignationName, cancellationToken);

                if (designation == null)
                {
                    throw new NotFoundException($"Designation '{request.DesignationName}' not found");
                }

                // 3. Check if designation already exists for this employee
                var existingDesignation = await _employeeDesignationRepo
                    .Query()
                    .FirstOrDefaultAsync(ed =>
                        ed.EmployeeId == employee.EmployeeId &&
                        ed.DesignationId == designation.DesignationId &&
                        (ed.EndDate == null || ed.EndDate >= request.StartDate),
                        cancellationToken);

                if (existingDesignation != null)
                {
                    throw new BadRequestException($"Employee {employee.EmployeeNumber} already has designation '{designation.Title}' assigned");
                }

                // 4. Create EmployeeDesignation record
                var employeeDesignation = new EmployeeDesignation
                {
                    EmployeeId = employee.EmployeeId,
                    DesignationId = designation.DesignationId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
                };

                await _employeeDesignationRepo.AddAsync(employeeDesignation);
                await _unitOfWork.SaveChangesAsync();

                // 5. Return success response
                return new AssignDesignationResponseDto
                {
                    Success = true,
                    Message = "Designation assigned successfully"
                };
        }
    }
}
