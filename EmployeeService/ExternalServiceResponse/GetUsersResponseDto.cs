namespace EmployeeServiceDepartmentService.ExternalServiceResponse
{
    public class GetUsersEnvelope
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<GetUsersResponseDto> Users { get; set; }
    }

    public class GetUsersResponseDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid RoleId { get; set; }
        public Guid? EmployeeId { get; set; }
        public int? EmployeeNumber { get; set; }
    }

}
