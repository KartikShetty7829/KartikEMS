namespace EmployeeServiceDepartmentService.domains
{
    public class GenerateLeaveSummaryReportResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public byte[] FileBytes { get; set; } // file content
        public string FileName { get; set; } // suggested download name
        public string ContentType { get; set; } // MIME type

    }
}
