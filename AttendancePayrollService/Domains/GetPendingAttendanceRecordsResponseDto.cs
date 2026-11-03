namespace AttendanceService.Domains
{

    public class GetPendingAttendanceRecordsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<AttendanceRecordDto> Records { get; set; } = new();
    }

    public class AttendanceRecordDto
    {
        public Guid RecordId { get; set; }
        public string FullName { get; set; } = string.Empty; // ✅ added full name
        public DateTime? CheckInAt { get; set; }
        public DateTime? CheckOutAt { get; set; }
       // public Guid? ApprovedBy { get; set; }
       // public DateTime? ApprovedAt { get; set; }
        public string? Status { get; set; }
    }
}
