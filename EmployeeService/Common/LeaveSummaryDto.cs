namespace EmployeeServiceDepartmentService.Common
{
    public class LeaveSummaryDto
    {
        public Guid LeaveId { get; set; }              // Unique ID of the leave request
        public  required string EmployeeName { get; set; }      // FirstName + LastName of employee
        public required string Type { get; set; }              // Leave type (e.g., Sick, Casual, Vacation)
        public DateTime StartDate { get; set; }       // Leave start date
        public DateTime EndDate { get; set; }         // Leave end date
        public required string Status { get; set; }            // Pending, Approved, Rejected
        public required string ApprovedByName { get; set; }    // Approver's full name (instead of ID)
    }

}
