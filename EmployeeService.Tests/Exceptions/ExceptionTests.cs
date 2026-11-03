using EmployeeServiceDepartmentService.Exceptions;
using EmployeeServiceDepartmentService.Models;
using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace EmployeeService.Tests.Exceptions
{
    public class ExceptionTests
    {
        [Fact]
        public void NotFoundException_SetsMessageCorrectly()
        {
            var ex = new NotFoundException("Employee not found");
            ex.Message.Should().Be("Employee not found");
        }

        [Fact]
        public void BadRequestException_SetsMessageCorrectly()
        {
            var ex = new BadRequestException("Invalid employee data");
            ex.Message.Should().Be("Invalid employee data");
        }

        [Fact]
        public void UnauthorizedException_SetsMessageCorrectly()
        {
            var ex = new UnauthorizedException("Unauthorized access");
            ex.Message.Should().Be("Unauthorized access");
        }

        [Fact]
        public void ValidationException_SetsMessageCorrectly()
        {
            var ex = new ValidationException("Model validation error");
            ex.Message.Should().Be("Model validation error");
        }

        [Fact]
        public void ErrorDetails_PropertiesAndSerialization_WorkCorrectly()
        {
            var details = new ErrorDetails
            {
                StatusCode = 404,
                Message = "Not Found",
                StackTrace = "Stack trace info"
            };

            details.StatusCode.Should().Be(404);
            details.Message.Should().Be("Not Found");
            details.StackTrace.Should().Be("Stack trace info");

            var json = JsonSerializer.Serialize(details);
            json.Should().Contain("404");
            json.Should().Contain("Not Found");
        }
    }
}
