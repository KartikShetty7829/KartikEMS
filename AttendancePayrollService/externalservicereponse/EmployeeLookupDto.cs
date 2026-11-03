namespace AttendancePayrollService.externalservicereponse
{
    public class EmployeeLookupDto
    {
        public Guid EmployeeId { get; set; }

        public Guid? ManagerId { get; set; }
        public int EmployeeNumber { get; set; }
        public string FullName { get; set; }   // 👈 keep this
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime HireDate { get; set; }
        public bool Status { get; set; }
    }
}
