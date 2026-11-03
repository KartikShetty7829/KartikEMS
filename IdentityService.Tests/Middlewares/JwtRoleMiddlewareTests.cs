using FluentAssertions;
using IdentityService.Middlewares;
using IdentityService.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace IdentityService.Tests.Middlewares
{
    public class JwtRoleMiddlewareTests
    {
        private const string SecretKey = "TestSuperSecretKeyForJwtRoleMiddleware123!";
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

        private string GenerateToken(string role, string username = "testuser")
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
        [InlineData("/swagger/index.html")]
        [InlineData("/health")]
        [InlineData("/api/identity/login")]
        [InlineData("/api/identity/adminlogin")]
        [InlineData("/api/identity/forgot-password")]
        public async Task InvokeAsync_PublicRoutes_SkipsTokenValidation(string path)
        {
            // Arrange
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

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            nextCalled.Should().BeTrue();
            context.Response.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task InvokeAsync_ProtectedRoute_NoAuthHeader_Returns401()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/identity/register";
            context.Response.Body = new MemoryStream();

            var nextCalled = false;
            RequestDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var logger = DbContextHelper.CreateLogger<JwtRoleMiddleware>();
            var middleware = new JwtRoleMiddleware(next, _config, logger);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            nextCalled.Should().BeFalse();
            context.Response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task InvokeAsync_ProtectedRoute_InvalidToken_Returns401()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/identity/register";
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

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            nextCalled.Should().BeFalse();
            context.Response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task InvokeAsync_ProtectedRoute_WrongRole_Returns403()
        {
            // Arrange
            var employeeToken = GenerateToken("Employee");
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/identity/register";
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

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            nextCalled.Should().BeFalse();
            context.Response.StatusCode.Should().Be(403);
        }

        [Fact]
        public async Task InvokeAsync_ProtectedRoute_CorrectAdminRole_PassesToNext()
        {
            // Arrange
            var adminToken = GenerateToken("Admin", "adminBoss");
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/identity/register";
            context.Request.Headers["Authorization"] = $"Bearer {adminToken}";

            var nextCalled = false;
            RequestDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var logger = DbContextHelper.CreateLogger<JwtRoleMiddleware>();
            var middleware = new JwtRoleMiddleware(next, _config, logger);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            nextCalled.Should().BeTrue();
            context.User.Should().NotBeNull();
            context.User.FindFirst(ClaimTypes.Name)?.Value.Should().Be("adminBoss");
            context.User.FindFirst(ClaimTypes.Role)?.Value.Should().Be("Admin");
        }
    }
}
