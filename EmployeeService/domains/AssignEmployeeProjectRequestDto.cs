using MediatR;

namespace EmployeeServiceDepartmentService.domains
{
    public class AssignEmployeeProjectRequestDto : IRequest<AssignEmployeeProjectResponseDto>
    {
        public int EmployeeNumber { get; set; }      // From UI
        public string ProjectName { get; set; }         // From UI
        public string DesignationTitle { get; set; }    // From UI
        public DateTime AssignedDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }

}
