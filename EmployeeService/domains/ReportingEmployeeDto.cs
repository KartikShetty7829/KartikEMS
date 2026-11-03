namespace EmployeeService.domains
{
    public class ReportingEmployeeDto
    {
        public Guid EmployeeId { get; set; }

        public int EmployeeNumber { get; set; }

        public string FullName { get; set; } = string.Empty;
    }
}
