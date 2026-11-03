using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using IdentityService.Domain;
using IdentityService.Middlewares;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;
using System.Text.Json;
using Xunit;

namespace IdentityService.Tests.Middlewares
{
    public class ValidationMiddlewareTests
    {
        private readonly Mock<IValidator<RegisterUserRequestDto>> _mockRegisterValidator;
        private readonly Mock<IValidator<AdminLoginRequestDto>> _mockAdminLoginValidator;

        public ValidationMiddlewareTests()
        {
            _mockRegisterValidator = new Mock<IValidator<RegisterUserRequestDto>>();
            _mockAdminLoginValidator = new Mock<IValidator<AdminLoginRequestDto>>();
        }

        [Fact]
        public async Task InvokeAsync_NonMonitoredPath_PassesToNext()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/other";
            var nextCalled = false;
            RequestDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new ValidationMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _mockRegisterValidator.Object, _mockAdminLoginValidator.Object);

            // Assert
            nextCalled.Should().BeTrue();
        }

        [Fact]
        public async Task InvokeAsync_RegisterPath_ValidModel_PassesToNext()
        {
            // Arrange
            var dto = new RegisterUserRequestDto { Username = "validUser", Password = "password123", Role = "Admin", EmployeeNumber = 101 };
            var json = JsonSerializer.Serialize(dto);
            var context = new DefaultHttpContext();
            context.Request.Path = "/register";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));

            _mockRegisterValidator
                .Setup(v => v.ValidateAsync(It.IsAny<RegisterUserRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var nextCalled = false;
            RequestDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new ValidationMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _mockRegisterValidator.Object, _mockAdminLoginValidator.Object);

            // Assert
            nextCalled.Should().BeTrue();
        }

        [Fact]
        public async Task InvokeAsync_RegisterPath_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var dto = new RegisterUserRequestDto { Username = "a", Password = "1", Role = "Invalid", EmployeeNumber = -1 };
            var json = JsonSerializer.Serialize(dto);
            var context = new DefaultHttpContext();
            context.Request.Path = "/register";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
            context.Response.Body = new MemoryStream();

            var failures = new List<ValidationFailure>
            {
                new ValidationFailure("Username", "Username is required")
            };
            _mockRegisterValidator
                .Setup(v => v.ValidateAsync(It.IsAny<RegisterUserRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));

            var nextCalled = false;
            RequestDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new ValidationMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _mockRegisterValidator.Object, _mockAdminLoginValidator.Object);

            // Assert
            nextCalled.Should().BeFalse();
            context.Response.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task InvokeAsync_AdminLoginPath_ValidModel_PassesToNext()
        {
            // Arrange
            var dto = new AdminLoginRequestDto { Username = "admin", Password = "password123" };
            var json = JsonSerializer.Serialize(dto);
            var context = new DefaultHttpContext();
            context.Request.Path = "/AdminLogin";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));

            _mockAdminLoginValidator
                .Setup(v => v.ValidateAsync(It.IsAny<AdminLoginRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var nextCalled = false;
            RequestDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new ValidationMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _mockRegisterValidator.Object, _mockAdminLoginValidator.Object);

            // Assert
            nextCalled.Should().BeTrue();
        }

        [Fact]
        public async Task InvokeAsync_AdminLoginPath_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var dto = new AdminLoginRequestDto { Username = "", Password = "" };
            var json = JsonSerializer.Serialize(dto);
            var context = new DefaultHttpContext();
            context.Request.Path = "/AdminLogin";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
            context.Response.Body = new MemoryStream();

            var failures = new List<ValidationFailure>
            {
                new ValidationFailure("Username", "Username is required")
            };
            _mockAdminLoginValidator
                .Setup(v => v.ValidateAsync(It.IsAny<AdminLoginRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));

            var nextCalled = false;
            RequestDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new ValidationMiddleware(next);

            // Act
            await middleware.InvokeAsync(context, _mockRegisterValidator.Object, _mockAdminLoginValidator.Object);

            // Assert
            nextCalled.Should().BeFalse();
            context.Response.StatusCode.Should().Be(400);
        }
    }
}
