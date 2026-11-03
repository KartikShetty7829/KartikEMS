namespace EmployeeServiceDepartmentService.domains
{
    public class GetPayrollRecordsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<PayrollRecordDto> PayrollRecords { get; set; } = new List<PayrollRecordDto>();
    }

    public class PayrollRecordDto
    {
        public Guid PayrollRecordId { get; set; }
        public int EmployeeNumber { get; set; }
        public string EmployeeName { get; set; }
        public int PayrollMonth { get; set; }
        public int PayrollYear { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal Allowances { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetPay { get; set; }
        public int WorkingDays { get; set; }
        public int PresentDays { get; set; }
        public DateTime GeneratedDate { get; set; }
    }
}
