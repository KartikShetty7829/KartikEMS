using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Exceptions;
using EmployeeServiceDepartmentService.Middlewares;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace EmployeeService.Tests.Middlewares
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
        public async Task InvokeAsync_WhenNoException_CallsNext()
        {
            var context = new DefaultHttpContext();
            var nextCalled = false;
            RequestDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var logger = DbContextHelper.CreateLogger<GlobalExceptionMiddleware>();
            var middleware = new GlobalExceptionMiddleware(next, logger, _mockEnv.Object);

            await middleware.InvokeAsync(context);

            nextCalled.Should().BeTrue();
            context.Response.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task InvokeAsync_WhenNotFoundException_Returns404()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = ctx => throw new NotFoundException("Employee not found");
            var logger = DbContextHelper.CreateLogger<GlobalExceptionMiddleware>();
            var middleware = new GlobalExceptionMiddleware(next, logger, _mockEnv.Object);

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task InvokeAsync_WhenBadRequestException_Returns400()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = ctx => throw new BadRequestException("Invalid employee data");
            var logger = DbContextHelper.CreateLogger<GlobalExceptionMiddleware>();
            var middleware = new GlobalExceptionMiddleware(next, logger, _mockEnv.Object);

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task InvokeAsync_WhenUnauthorizedException_Returns401()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = ctx => throw new UnauthorizedException("Unauthorized operation");
            var logger = DbContextHelper.CreateLogger<GlobalExceptionMiddleware>();
            var middleware = new GlobalExceptionMiddleware(next, logger, _mockEnv.Object);

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task InvokeAsync_WhenValidationException_Returns400()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = ctx => throw new ValidationException("Invalid model");
            var logger = DbContextHelper.CreateLogger<GlobalExceptionMiddleware>();
            var middleware = new GlobalExceptionMiddleware(next, logger, _mockEnv.Object);

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task InvokeAsync_WhenGenericException_Returns500()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = ctx => throw new InvalidOperationException("Fatal database error");
            var logger = DbContextHelper.CreateLogger<GlobalExceptionMiddleware>();
            var middleware = new GlobalExceptionMiddleware(next, logger, _mockEnv.Object);

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be(500);
        }
    }
}
