using MediatR;
using System.Collections.Generic;

namespace EmployeeServiceDepartmentService.domains
{
    // Bulk request: admin can send multiple designations at once
    public class CreateDesignationsRequestDto : IRequest<CreateDesignationResponseDto>
    {
        public required List<SingleDesignationDto> Designations { get; set; }
    }

    // Represents one designation inside the bulk request
    public class SingleDesignationDto
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
    }
}
