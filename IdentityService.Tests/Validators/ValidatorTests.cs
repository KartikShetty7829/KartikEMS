using FluentAssertions;
using IdentityService.Domain;
using IdentityService.Validators;
using Xunit;

namespace IdentityService.Tests.Validators
{
    public class ValidatorTests
    {
        private readonly AdminLoginRequestDtoValidator _adminLoginValidator;
        private readonly RegisterUserRequestDtoValidator _registerUserValidator;

        public ValidatorTests()
        {
            _adminLoginValidator = new AdminLoginRequestDtoValidator();
            _registerUserValidator = new RegisterUserRequestDtoValidator();
        }

        [Fact]
        public void AdminLoginRequestDtoValidator_ValidInput_PassesValidation()
        {
            // Arrange
            var request = new AdminLoginRequestDto
            {
                Username = "adminUser",
                Password = "validPassword123"
            };

            // Act
            var result = _adminLoginValidator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("", "validPassword123")]
        [InlineData("ab", "validPassword123")]
        [InlineData("validUser", "")]
        [InlineData("validUser", "12345")]
        public void AdminLoginRequestDtoValidator_InvalidInput_FailsValidation(string username, string password)
        {
            // Arrange
            var request = new AdminLoginRequestDto
            {
                Username = username,
                Password = password
            };

            // Act
            var result = _adminLoginValidator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void RegisterUserRequestDtoValidator_ValidAdminRequest_PassesValidation()
        {
            // Arrange
            var request = new RegisterUserRequestDto
            {
                Username = "validUser",
                Password = "validPassword123",
                EmployeeNumber = 101,
                Role = "Admin"
            };

            // Act
            var result = _registerUserValidator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void RegisterUserRequestDtoValidator_ValidEmployeeRequest_PassesValidation()
        {
            // Arrange
            var request = new RegisterUserRequestDto
            {
                Username = "validEmployee",
                Password = "validPassword123",
                EmployeeNumber = 102,
                Role = "Employee"
            };

            // Act
            var result = _registerUserValidator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("InvalidRole")]
        [InlineData("Manager")]
        [InlineData("SuperAdmin")]
        public void RegisterUserRequestDtoValidator_InvalidRole_FailsValidation(string invalidRole)
        {
            // Arrange
            var request = new RegisterUserRequestDto
            {
                Username = "validUser",
                Password = "validPassword123",
                EmployeeNumber = 101,
                Role = invalidRole
            };

            // Act
            var result = _registerUserValidator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Role must be either 'Admin' or 'User'");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void RegisterUserRequestDtoValidator_NonPositiveEmployeeNumber_FailsValidation(int invalidEmpNumber)
        {
            // Arrange
            var request = new RegisterUserRequestDto
            {
                Username = "validUser",
                Password = "validPassword123",
                EmployeeNumber = invalidEmpNumber,
                Role = "Employee"
            };

            // Act
            var result = _registerUserValidator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Employee number must be positive");
        }
    }
}
