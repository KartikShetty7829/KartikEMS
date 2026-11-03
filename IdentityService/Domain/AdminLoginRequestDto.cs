using MediatR;

namespace IdentityService.Domain
{
    public class AdminLoginRequestDto : IRequest<AdminLoginResponseDto>
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
