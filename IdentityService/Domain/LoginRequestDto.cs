using MediatR;

namespace IdentityService.Domain
{
    // Request DTO
    public class LoginRequestDto : IRequest<LoginResponseDto>
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}