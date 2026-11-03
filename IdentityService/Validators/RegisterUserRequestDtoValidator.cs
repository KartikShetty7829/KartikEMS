using FluentValidation;
using IdentityService.Domain;

namespace IdentityService.Validators
{

    public class RegisterUserRequestDtoValidator : AbstractValidator<RegisterUserRequestDto>
    {
        public RegisterUserRequestDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters long");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long");

            RuleFor(x => x.EmployeeNumber)
           .GreaterThan(0)
           .WithMessage("Employee number must be positive");


            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required")
                .Must(role => role == "Admin" || role == "Employee")
                .WithMessage("Role must be either 'Admin' or 'User'");
        }
    }

}
