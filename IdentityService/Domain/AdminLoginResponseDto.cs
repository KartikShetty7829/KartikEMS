namespace IdentityService.Domain
{
    public class AdminLoginResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? Token { get; set; }      // JWT token returned on success
        public string? Role { get; set; }        // Role name (e.g. "Admin")
        public string? Username { get; set; }     
        public Guid AdminUserId { get; set; }

        public int EmployeeNumber { get; set; } // Logged-in username
    }
}
