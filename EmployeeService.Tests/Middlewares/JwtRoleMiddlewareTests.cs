using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Middlewares;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace EmployeeService.Tests.Middlewares
{
    public class JwtRoleMiddlewareTests
    {
        private const string SecretKey = "EmployeeServiceJwtRoleMiddlewareSecretKey12345!";
        private readonly IConfiguration _config;

        public JwtRoleMiddlewareTests()
        {
            var inMemoryConfig = new Dictionary<string, string?>
            {
                { "Jwt:Key", SecretKey },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" }
            };
            _config = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();
        }

        private string GenerateToken(string role, string username = "employeeUser")
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: "TestIssuer",
                audience: "TestAudience",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Theory]
        [InlineData("/swagger/v1/swagger.json")]
        [InlineData("/health")]
        public async Task InvokeAsync_SwaggerAndHealthRoutes_SkipsValidation(string path)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = path;

            var nextCalled = false;
            RequestDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var logger = DbContextHelper.CreateLogger<JwtRoleMiddleware>();
            var middleware = new JwtRoleMiddleware(next, _config, logger);

            await middleware.InvokeAsync(context);

            nextCalled.Should().BeTrue();
            context.Response.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task InvokeAsync_NonAdminRoute_PassesThrough()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/employee/other-endpoint";

            var nextCalled = false;
            RequestDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var logger = DbContextHelper.CreateLogger<JwtRoleMiddleware>();
            var middleware = new JwtRoleMiddleware(next, _config, logger);

            await middleware.InvokeAsync(context);

            nextCalled.Should().BeTrue();
        }

        [Fact]
        public async Task InvokeAsync_AdminRoute_NoToken_Returns401()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/employee/add";
            context.Response.Body = new MemoryStream();

            var nextCalled = false;
            RequestDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var logger = DbContextHelper.CreateLogger<JwtRoleMiddleware>();
            var middleware = new JwtRoleMiddleware(next, _config, logger);

            await middleware.InvokeAsync(context);

            nextCalled.Should().BeFalse();
            context.Response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task InvokeAsync_AdminRoute_InvalidToken_Returns401()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/employee/add";
            context.Request.Headers["Authorization"] = "Bearer invalid.jwt.token";
            context.Response.Body = new MemoryStream();

            var nextCalled = false;
            RequestDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var logger = DbContextHelper.CreateLogger<JwtRoleMiddleware>();
            var middleware = new JwtRoleMiddleware(next, _config, logger);

            await middleware.InvokeAsync(context);

            nextCalled.Should().BeFalse();
            context.Response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task InvokeAsync_AdminRoute_EmployeeRole_Returns403()
        {
            var employeeToken = GenerateToken("Employee");
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/employee/add";
            context.Request.Headers["Authorization"] = $"Bearer {employeeToken}";
            context.Response.Body = new MemoryStream();

            var nextCalled = false;
            RequestDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var logger = DbContextHelper.CreateLogger<JwtRoleMiddleware>();
            var middleware = new JwtRoleMiddleware(next, _config, logger);

            await middleware.InvokeAsync(context);

            nextCalled.Should().BeFalse();
            context.Response.StatusCode.Should().Be(403);
        }

        [Fact]
        public async Task InvokeAsync_AdminRoute_AdminRole_PassesThrough()
        {
            var adminToken = GenerateToken("Admin", "adminBoss");
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/employee/add";
            context.Request.Headers["Authorization"] = $"Bearer {adminToken}";

            var nextCalled = false;
            RequestDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var logger = DbContextHelper.CreateLogger<JwtRoleMiddleware>();
            var middleware = new JwtRoleMiddleware(next, _config, logger);

            await middleware.InvokeAsync(context);

            nextCalled.Should().BeTrue();
            context.User.Should().NotBeNull();
            context.User.FindFirst(ClaimTypes.Role)?.Value.Should().Be("Admin");
        }
    }
}
