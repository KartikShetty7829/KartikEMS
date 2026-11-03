namespace EmployeeService.domains
{
    public class EmployeeDesignationResponseDto
    {
        public int EmployeeNumber { get; set; }
        public string DesignationName { get; set; }

        public DateTime StartDate { get; set; }   // ✅ added
        public DateTime? EndDate { get; set; }    // ✅ added

    }
}
