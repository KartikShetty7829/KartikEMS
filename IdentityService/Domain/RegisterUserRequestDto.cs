using MediatR;
using System;

namespace IdentityService.Domain
{
    public class RegisterUserRequestDto : IRequest<RegisterUserResponseDto>
    {
        public required  string Username { get; set; }
        public required  string Password { get; set; }
        public required int EmployeeNumber { get; set; }
        public required string Role { get; set; }
    }
}
