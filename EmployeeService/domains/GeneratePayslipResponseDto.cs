namespace EmployeeServiceDepartmentService.domains
{
    public class GeneratePayslipResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string FilePath { get; set; }  // Path to generated PDF
    }
}

