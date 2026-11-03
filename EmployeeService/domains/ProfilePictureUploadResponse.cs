namespace EmployeeServiceDepartmentService.domains
{
    public class ProfilePictureUploadResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public string ProfilePictureUrl { get; set; }
        public bool Success { get; internal set; }
    }

}
