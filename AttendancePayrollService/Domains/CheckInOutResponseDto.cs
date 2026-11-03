namespace AttendancePayrollService.Domains
{
    public class CheckInOutResponseDto
    {
        public bool Success { get; set; } 

        public string Message { get; set; } // Always true for success public string Message { get; set; } = "Applied successfully";
    }
}
