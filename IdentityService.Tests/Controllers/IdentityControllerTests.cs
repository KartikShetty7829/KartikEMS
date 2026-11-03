using FluentAssertions;
using IdentityService.Domain;
using IdentityService.Tests.TestHelpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace IdentityService.Tests.Controllers
{
    public class IdentityControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly IdentityController _controller;

        public IdentityControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            var logger = DbContextHelper.CreateLogger<IdentityController>();
            _controller = new IdentityController(_mockMediator.Object, logger);
        }

        [Fact]
        public async Task Register_WhenSuccess_ReturnsOk()
        {
            // Arrange
            var request = new RegisterUserRequestDto { Username = "user", Password = "password", Role = "Employee", EmployeeNumber = 101 };
            var response = new RegisterUserResponseDto { Success = true, Message = "Success" };

            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Register(request);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task Register_WhenFailed_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterUserRequestDto { Username = "user", Password = "password", Role = "Employee", EmployeeNumber = 101 };
            var response = new RegisterUserResponseDto { Success = false, Message = "Failed" };

            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Register(request);

            // Assert
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task Login_WhenSuccess_ReturnsOk()
        {
            // Arrange
            var request = new LoginRequestDto { Username = "emp", Password = "password" };
            var response = new LoginResponseDto { Success = true, Token = "token123" };

            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task Login_WhenFailed_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequestDto { Username = "emp", Password = "password" };
            var response = new LoginResponseDto { Success = false, Message = "Invalid" };

            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorizedResult.StatusCode.Should().Be(401);
            unauthorizedResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task AdminLogin_WhenRequestNull_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.AdminLogin(null!);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("Invalid request.");
        }

        [Fact]
        public async Task AdminLogin_WhenSuccess_ReturnsOk()
        {
            // Arrange
            var request = new AdminLoginRequestDto { Username = "admin", Password = "password" };
            var response = new AdminLoginResponseDto { Success = true, Token = "adminToken" };

            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.AdminLogin(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task AdminLogin_WhenFailed_ReturnsUnauthorized()
        {
            // Arrange
            var request = new AdminLoginRequestDto { Username = "admin", Password = "password" };
            var response = new AdminLoginResponseDto { Success = false, Message = "Failed" };

            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.AdminLogin(request);

            // Assert
            var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorizedResult.StatusCode.Should().Be(401);
            unauthorizedResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task ForgotPassword_ReturnsOkWithResponse()
        {
            // Arrange
            var request = new ForgotPasswordRequestDto { Username = "test", NewPassword = "new" };
            var response = new ForgotPasswordResponseDto { Success = true, Message = "Reset" };

            _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.ForgotPassword(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(response);
        }
    }
}
