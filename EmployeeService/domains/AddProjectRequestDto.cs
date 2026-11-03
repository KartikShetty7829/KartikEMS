using MediatR;

namespace EmployeeServiceDepartmentService.domains
{
    public class AddProjectRequestDto : IRequest<AddProjectResponseDto>
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
    }

}
