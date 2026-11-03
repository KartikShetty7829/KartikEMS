using MediatR;

namespace EmployeeServiceDepartmentService.domains
{
    public class UpdateLeaveStatusRequestDto : IRequest<UpdateLeaveStatusResponseDto>
    {
        public Guid LeaveId { get; set; }
        public int  AdminEmployeeNumber { get; set; }
        public required string Action { get; set; }  // "Approve" or "Reject"
    }

}
