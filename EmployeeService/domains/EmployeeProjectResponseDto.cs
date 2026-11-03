namespace EmployeeService.domains
{
    public class EmployeeProjectResponseDto
    {
        public int EmployeeNumber { get; set; }
        public List<ProjectDto> Projects { get; set; } = new();
    }

    public class ProjectDto
    {
        
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
    }
}
