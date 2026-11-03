using EmployeeServiceDepartmentService.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace EmployeeService.Tests.Controllers
{
    public class EmployeeV2ControllerTests
    {
        [Fact]
        public void GetHelloV2_ReturnsOkWithExpectedMessage()
        {
            // Arrange
            var controller = new EmployeeV2Controller();

            // Act
            var result = controller.GetHelloV2();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().NotBeNull();
        }
    }
}
