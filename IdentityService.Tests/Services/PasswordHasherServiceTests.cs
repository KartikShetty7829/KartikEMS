using FluentAssertions;
using IdentityService.Services;
using Xunit;

namespace IdentityService.Tests.Services
{
    public class PasswordHasherServiceTests
    {
        private readonly PasswordHasherService _hasher;

        public PasswordHasherServiceTests()
        {
            _hasher = new PasswordHasherService();
        }

        [Fact]
        public void HashPassword_ReturnsThreePartHashedString()
        {
            // Act
            var hash = _hasher.HashPassword("MySecurePassword123!");

            // Assert
            hash.Should().NotBeNullOrWhiteSpace();
            var parts = hash.Split('.');
            parts.Should().HaveCount(3);
            parts[0].Should().Be("10000"); // Iterations
            parts[1].Should().NotBeEmpty(); // Salt
            parts[2].Should().NotBeEmpty(); // Key
        }

        [Fact]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            var password = "SuperSecretPassword#2026";
            var hashedPassword = _hasher.HashPassword(password);

            // Act
            var isMatch = _hasher.VerifyPassword(password, hashedPassword);

            // Assert
            isMatch.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_WrongPassword_ReturnsFalse()
        {
            // Arrange
            var password = "SuperSecretPassword#2026";
            var hashedPassword = _hasher.HashPassword(password);

            // Act
            var isMatch = _hasher.VerifyPassword("WrongPassword123", hashedPassword);

            // Assert
            isMatch.Should().BeFalse();
        }

        [Fact]
        public void VerifyPassword_PlainTextFallback_MatchesPlainTextPassword()
        {
            // Arrange (unhashed legacy plain text password)
            var plainText = "legacyPassword123";

            // Act
            var isMatch = _hasher.VerifyPassword("legacyPassword123", plainText);

            // Assert
            isMatch.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_PlainTextFallback_WrongPlainTextReturnsFalse()
        {
            // Arrange
            var plainText = "legacyPassword123";

            // Act
            var isMatch = _hasher.VerifyPassword("differentPassword", plainText);

            // Assert
            isMatch.Should().BeFalse();
        }
    }
}
