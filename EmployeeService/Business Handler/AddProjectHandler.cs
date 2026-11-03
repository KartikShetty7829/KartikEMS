using EmployeeManagementSystem.Entities;
using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Exceptions;
using EmployeeServiceDepartmentService.interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EmployeeServiceDepartmentService.Business_Handler
{
    public class AddProjectHandler
        : IRequestHandler<AddProjectRequestDto, AddProjectResponseDto>
    {
        private readonly IGenericRepository<EmpDepartmentDbContext, Projects> _projectRepo;
        private readonly IUnitOfWork<EmpDepartmentDbContext> _unitOfWork;
        private readonly ILogger<AddProjectHandler> _logger;

        public AddProjectHandler(
            IGenericRepository<EmpDepartmentDbContext, Projects> projectRepo,
            IUnitOfWork<EmpDepartmentDbContext> unitOfWork,
            ILogger<AddProjectHandler> logger)
        {
            _projectRepo = projectRepo;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<AddProjectResponseDto> Handle(AddProjectRequestDto request, CancellationToken cancellationToken)
        {
                // 1. Create Project entity from request
                var projectEntity = new Projects
                {
                    
                    Name = request.Name,
                    Description = request.Description,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Status = request.Status
                };

                // 2. Save using repository + unit of work
                await _projectRepo.AddAsync(projectEntity);
                await _unitOfWork.SaveChangesAsync();

                // 3. Return success response
                return new AddProjectResponseDto
                {
                    Success = true,
                    Message = "Project created successfully",
                   
                };
        }
    }
}

