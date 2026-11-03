namespace EmployeeServiceDepartmentService.domains
{
    public class GetEmployeeLeavesResponseDto
    {
        public List<EmployeeLeaveDto> Leaves { get; set; }
        public string Message { get; set; }
    }

    public class EmployeeLeaveDto
    {
        public Guid LeaveId { get; set; }
        public string Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public string ApprovedByName { get; set; }
    }
}
