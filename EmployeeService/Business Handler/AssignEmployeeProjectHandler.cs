using EmployeeManagementSystem.Entities;
using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Exceptions;
using EmployeeServiceDepartmentService.interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeServiceDepartmentService.Business_Handler
{
    public class AssignEmployeeProjectHandler
        : IRequestHandler<AssignEmployeeProjectRequestDto, AssignEmployeeProjectResponseDto>
    {
        private readonly IGenericRepository<EmpDepartmentDbContext, Employees> _employeeRepo;
        private readonly IGenericRepository<EmpDepartmentDbContext, Projects> _projectRepo;
        private readonly IGenericRepository<EmpDepartmentDbContext, Designation> _designationRepo;
        private readonly IGenericRepository<EmpDepartmentDbContext, EmployeeProject> _employeeProjectRepo;
        private readonly IUnitOfWork<EmpDepartmentDbContext> _unitOfWork;
        private readonly ILogger<AssignEmployeeProjectHandler> _logger;

        public AssignEmployeeProjectHandler(
            IGenericRepository<EmpDepartmentDbContext, Employees> employeeRepo,
            IGenericRepository<EmpDepartmentDbContext, Projects> projectRepo,
            IGenericRepository<EmpDepartmentDbContext, Designation> designationRepo,
            IGenericRepository<EmpDepartmentDbContext, EmployeeProject> employeeProjectRepo,
            IUnitOfWork<EmpDepartmentDbContext> unitOfWork,
            ILogger<AssignEmployeeProjectHandler> logger)
        {
            _employeeRepo = employeeRepo;
            _projectRepo = projectRepo;
            _designationRepo = designationRepo;
            _employeeProjectRepo = employeeProjectRepo;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<AssignEmployeeProjectResponseDto> Handle(AssignEmployeeProjectRequestDto request, CancellationToken cancellationToken)
        {
                // 1. Lookup Employee by EmployeeNumber
                var employee = await _employeeRepo.Query()
                    .FirstOrDefaultAsync(e => e.EmployeeNumber == request.EmployeeNumber, cancellationToken);

                if (employee == null)
                {
                    throw new NotFoundException($"Employee with number {request.EmployeeNumber} not found");
                }

                // 2. Lookup Project by ProjectName
                var project = await _projectRepo.Query()
                    .FirstOrDefaultAsync(p => p.Name == request.ProjectName, cancellationToken);

                if (project == null)
                {
                    throw new NotFoundException($"Project with name {request.ProjectName} not found");
                }

                // 3. Lookup Designation by Title
                var designation = await _designationRepo.Query()
                    .FirstOrDefaultAsync(d => d.Title == request.DesignationTitle, cancellationToken);

                if (designation == null)
                {
                    throw new NotFoundException($"Designation with title {request.DesignationTitle} not found");
                }

                // 4. Create EmployeeProject entity
                var employeeProject = new EmployeeProject
                {
                    EmployeeId = employee.EmployeeId,
                    ProjectId = project.ProjectId,
                    DesignationId = designation.DesignationId,
                    AssignedDate = request.AssignedDate,
                    ReleaseDate = request.ReleaseDate
                };

                await _employeeProjectRepo.AddAsync(employeeProject);
                await _unitOfWork.SaveChangesAsync();

                // 5. Return success response
                return new AssignEmployeeProjectResponseDto
                {
                    Success = true,
                    Message = "Employee assigned to project successfully",          
                };
        }
    }
}
