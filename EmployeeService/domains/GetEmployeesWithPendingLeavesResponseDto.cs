namespace EmployeeServiceDepartmentService.domains
{
    public class GetEmployeesWithPendingLeavesResponseDto
    {
        //public bool Success { get; set; }
        //public string Message { get; set; } 
        public List<EmployeePendingLeaveDto> PendingLeaves { get; set; }
    }

    // Employee + Leave info DTO
    public class EmployeePendingLeaveDto
    {
        
            public Guid LeaveId { get; set; }
            public required string EmployeeName { get; set; } 
            public string Type { get; set; } 
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public required string Status { get; set; } 
            public string? ApprovedByName { get; set; } // null if not approved yet
        
    }
}
