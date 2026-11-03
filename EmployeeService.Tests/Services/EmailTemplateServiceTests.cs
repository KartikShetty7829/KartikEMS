using FluentAssertions;
using Xunit;

namespace EmployeeService.Tests.Services
{
    public class EmailTemplateServiceTests
    {
        [Fact]
        public void GetWelcomeEmployeeTemplate_ReplacesPlaceholdersCorrectly()
        {
            // Arrange
            var hireDate = new DateTime(2026, 1, 15);

            // Act
            var template = EmailTemplateService.GetWelcomeEmployeeTemplate(
                "John",
                "Doe",
                "john.doe@example.com",
                "1234567890",
                hireDate);

            // Assert
            template.Should().NotBeNullOrWhiteSpace();
            template.Should().Contain("John");
            template.Should().Contain("Doe");
            template.Should().Contain("john.doe@example.com");
            template.Should().Contain("1234567890");
            template.Should().Contain(hireDate.ToString("dd-MMM-yyyy"));
        }
    }
}
