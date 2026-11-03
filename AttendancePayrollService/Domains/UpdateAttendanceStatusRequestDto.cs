using MediatR;

namespace AttendanceService.Domains
{
    public class UpdateAttendanceStatusRequestDto: IRequest<UpdateAttendanceStatusResponseDto>
    {
        public Guid RecordId { get; set; }

        public string AdminUserName { get; set; }

        public int EmployeeNumber { get; set; }

        public required string Action { get; set; }
    }
}
