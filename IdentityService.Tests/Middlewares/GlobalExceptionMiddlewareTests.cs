using FluentAssertions;
using IdentityService.Exceptions;
using IdentityService.Middlewares;
using IdentityService.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace IdentityService.Tests.Middlewares
{
    public class GlobalExceptionMiddlewareTests
    {
        private readonly Mock<IHostEnvironment> _mockEnv;

        public GlobalExceptionMiddlewareTests()
        {
            _mockEnv = new Mock<IHostEnvironment>();
            _mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        }

        [Fact]
        public async Task InvokeAsync_WhenNoException_CallsNextDelegate()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var nextCalled = false;
            RequestDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var logger = DbContextHelper.CreateLogger<GlobalExceptionMiddleware>();
            var middleware = new GlobalExceptionMiddleware(next, logger, _mockEnv.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            nextCalled.Should().BeTrue();
            context.Response.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task InvokeAsync_WhenNotFoundException_Returns404()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = ctx => throw new NotFoundException("User not found");
            var logger = DbContextHelper.CreateLogger<GlobalExceptionMiddleware>();
            var middleware = new GlobalExceptionMiddleware(next, logger, _mockEnv.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be(404);
            context.Response.ContentType.Should().Contain("application/json");
        }

        [Fact]
        public async Task InvokeAsync_WhenBadRequestException_Returns400()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = ctx => throw new BadRequestException("Username already exists");
            var logger = DbContextHelper.CreateLogger<GlobalExceptionMiddleware>();
            var middleware = new GlobalExceptionMiddleware(next, logger, _mockEnv.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task InvokeAsync_WhenUnauthorizedException_Returns401()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = ctx => throw new UnauthorizedException("Invalid credentials");
            var logger = DbContextHelper.CreateLogger<GlobalExceptionMiddleware>();
            var middleware = new GlobalExceptionMiddleware(next, logger, _mockEnv.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task InvokeAsync_WhenValidationException_Returns400()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = ctx => throw new ValidationException("Invalid model");
            var logger = DbContextHelper.CreateLogger<GlobalExceptionMiddleware>();
            var middleware = new GlobalExceptionMiddleware(next, logger, _mockEnv.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task InvokeAsync_WhenGenericException_Returns500()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = ctx => throw new InvalidOperationException("Fatal error");
            var logger = DbContextHelper.CreateLogger<GlobalExceptionMiddleware>();
            var middleware = new GlobalExceptionMiddleware(next, logger, _mockEnv.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be(500);
        }
    }
}
