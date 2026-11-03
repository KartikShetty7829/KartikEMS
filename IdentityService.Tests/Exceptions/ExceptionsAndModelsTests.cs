using FluentAssertions;
using IdentityService.Exceptions;
using IdentityService.Models;
using System.Text.Json;
using Xunit;

namespace IdentityService.Tests.Exceptions
{
    public class ExceptionsAndModelsTests
    {
        [Fact]
        public void NotFoundException_SetsMessageCorrectly()
        {
            var ex = new NotFoundException("Custom not found message");
            ex.Message.Should().Be("Custom not found message");
        }

        [Fact]
        public void BadRequestException_SetsMessageCorrectly()
        {
            var ex = new BadRequestException("Custom bad request message");
            ex.Message.Should().Be("Custom bad request message");
        }

        [Fact]
        public void UnauthorizedException_SetsMessageCorrectly()
        {
            var ex = new UnauthorizedException("Custom unauthorized message");
            ex.Message.Should().Be("Custom unauthorized message");
        }

        [Fact]
        public void ValidationException_SetsMessageCorrectly()
        {
            var ex = new ValidationException("Custom validation message");
            ex.Message.Should().Be("Custom validation message");
        }

        [Fact]
        public void ErrorDetails_PropertiesAndSerialization_WorkCorrectly()
        {
            var error = new ErrorDetails
            {
                StatusCode = 404,
                Message = "Not Found",
                StackTrace = "Stack trace line"
            };

            error.StatusCode.Should().Be(404);
            error.Message.Should().Be("Not Found");
            error.StackTrace.Should().Be("Stack trace line");

            var json = JsonSerializer.Serialize(error);
            json.Should().Contain("404");
            json.Should().Contain("Not Found");
        }
    }
}
