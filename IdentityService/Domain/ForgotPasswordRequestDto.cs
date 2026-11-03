using MediatR;

namespace IdentityService.Domain
{
    public class ForgotPasswordRequestDto : IRequest<ForgotPasswordResponseDto>
    {
        public string Username { get; set; }
        public string NewPassword { get; set; }
    }
}
