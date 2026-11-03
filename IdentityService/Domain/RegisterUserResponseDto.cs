namespace IdentityService.Domain
{
    public class RegisterUserResponseDto
    {
        public Guid UserId { get; set; }

        public string Username { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Role { get; set; }

        public bool Success { get; set; }

        public string Message { get; set; }

        public int? EmployeeNumber { get; set; }

    }
}
