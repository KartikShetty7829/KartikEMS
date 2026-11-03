using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeServiceDepartmentService.Business_Handler
{
    // Bulk handler: returns overall success/failure
    public class CreateDesignationsHandler : IRequestHandler<CreateDesignationsRequestDto, CreateDesignationResponseDto>
    {
        private readonly IGenericRepository<EmpDepartmentDbContext, Designation> _designationRepo;
        private readonly ILogger<CreateDesignationsHandler> _logger;
        private readonly IUnitOfWork<EmpDepartmentDbContext> _unitOfWork;

        public CreateDesignationsHandler(
            IGenericRepository<EmpDepartmentDbContext, Designation> designationRepo,
            ILogger<CreateDesignationsHandler> logger,
            IUnitOfWork<EmpDepartmentDbContext> unitOfWork)
        {
            _designationRepo = designationRepo;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<CreateDesignationResponseDto> Handle(CreateDesignationsRequestDto request, CancellationToken cancellationToken)
        {
            var response = new CreateDesignationResponseDto();
            _logger.LogInformation("Bulk creating {Count} designations", request.Designations.Count);

            bool allSucceeded = true;

            foreach (var dto in request.Designations)
            {
                    var designationEntity = new Designation
                    {
                        Title = dto.Title,
                        Description = dto.Description
                    };

                    await _designationRepo.AddAsync(designationEntity);
                    await _unitOfWork.SaveChangesAsync();
            }
            response.Success = allSucceeded;
            response.Message = allSucceeded
                ? "All designations created successfully"
                : "Some designations failed";
            return response;
        }
    }
}
