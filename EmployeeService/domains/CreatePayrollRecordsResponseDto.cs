namespace EmployeeServiceDepartmentService.domains
{
    public class CreatePayrollRecordsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int RecordsGenerated { get; set; }
    }
}
