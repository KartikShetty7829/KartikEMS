using MediatR;

namespace IdentityService.Domain
{
    // Request DTO
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int EmployeeNumber { get; set; }

        public string? Token { get; set; }      // JWT token returned on success
        public string? Role { get; set; }        // Role name (e.g. "Admin")
        public string? Username { get; set; }

        public Guid UserId { get; set; }
    }
}