using System.ComponentModel.DataAnnotations.Schema;

namespace AttendancePayrollService.Domains
{
    public class GetAttendanceRecordsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<AttendanceRecordDto> Records { get; set; } = new();
    }

    // Record DTO (simplified for frontend)
    public class AttendanceRecordDto
    {
        public DateTime? CheckInAt { get; set; }
        public DateTime? CheckOutAt { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public string? Status { get; set; }

    }

}
