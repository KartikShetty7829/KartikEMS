using System;

namespace EmployeeServiceDepartmentService.domains
{
    public class DefinePayrollResponseDto
    {
        public bool Success { get; set; }             // Operation result
        public string Message { get; set; }           // Info message
        //public Guid? PayrollRecordId { get; set; }    // Updated record ID
        //public decimal? NetPay { get; set; }          // Computed net pay
    }
}

