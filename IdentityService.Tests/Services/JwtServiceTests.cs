using FluentAssertions;
using IdentityService.Entities;
using IdentityService.Services;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace IdentityService.Tests.Services
{
    public class JwtServiceTests
    {
        private const string TestSecretKey = "SuperSecretKeyForTestingJwtService1234567890!";

        [Fact]
        public void GenerateJwtToken_ValidUserAndRole_GeneratesValidJwtTokenWithClaims()
        {
            // Arrange
            var configValues = new Dictionary<string, string?>
            {
                { "Jwt:Key", TestSecretKey },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:ExpiryHours", "4" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();

            var jwtService = new JwtService(configuration);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "testUser",
                Password = "hashedPassword"
            };

            // Act
            var tokenString = jwtService.GenerateJwtToken(user, "Admin");

            // Assert
            tokenString.Should().NotBeNullOrWhiteSpace();

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(tokenString);

            token.Issuer.Should().Be("TestIssuer");
            token.Audiences.Should().Contain("TestAudience");
            token.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value.Should().Be(user.UserId.ToString());
            token.Claims.First(c => c.Type == ClaimTypes.Name).Value.Should().Be("testUser");
            token.Claims.First(c => c.Type == ClaimTypes.Role).Value.Should().Be("Admin");
        }

        [Fact]
        public void GenerateJwtToken_DefaultExpiry_WhenExpiryHoursNotSpecified()
        {
            // Arrange
            var configValues = new Dictionary<string, string?>
            {
                { "Jwt:Key", TestSecretKey },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" }
                // ExpiryHours omitted
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();

            var jwtService = new JwtService(configuration);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "testUser",
                Password = "hashedPassword"
            };

            // Act
            var tokenString = jwtService.GenerateJwtToken(user, "Employee");

            // Assert
            tokenString.Should().NotBeNullOrWhiteSpace();

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(tokenString);

            token.ValidTo.Should().BeAfter(DateTime.UtcNow.AddHours(7));
        }
    }
}
