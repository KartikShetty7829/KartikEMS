namespace EmployeeServiceDepartmentService.domains
{
    public class ApplyLeaveResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } 

        public string Email { get; set; }

        public string Phone { get; set; }

        public string EmployeeName {  get; set; }
    }
}
