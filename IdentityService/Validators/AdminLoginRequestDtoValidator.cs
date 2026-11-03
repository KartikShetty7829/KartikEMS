using FluentValidation;
using IdentityService.Domain;

namespace IdentityService.Validators
{
    public class AdminLoginRequestDtoValidator : AbstractValidator<AdminLoginRequestDto>
    {
        public AdminLoginRequestDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters long");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long");
        }
    }

}
