namespace EmployeeServiceDepartmentService.domains
{
    public class EmployeeResponseDto
    {
        public Guid? EmployeeId { get; set; }

        public int EmployeeNumber { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
