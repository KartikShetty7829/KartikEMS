using EmployeeServiceDepartmentService.External_Service;
using EmployeeServiceDepartmentService.ExternalServiceResponse;
using FluentAssertions;
using System.Net;
using System.Text.Json;
using Xunit;

namespace EmployeeService.Tests.Services
{
    public class UserServiceClientTests
    {
        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

            public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
            {
                _handler = handler;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_handler(request));
            }
        }

        [Fact]
        public async Task GetUsersAsync_WhenSuccessful_ReturnsListOfUsers()
        {
            // Arrange
            var envelope = new GetUsersEnvelope
            {
                Users = new List<GetUsersResponseDto>
                {
                    new GetUsersResponseDto { Username = "alice", RoleId = Guid.NewGuid(), EmployeeNumber = 101 },
                    new GetUsersResponseDto { Username = "bob", RoleId = Guid.NewGuid(), EmployeeNumber = 102 }
                }
            };

            var fakeHandler = new FakeHttpMessageHandler(req =>
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(envelope), System.Text.Encoding.UTF8, "application/json")
                };
            });

            var httpClient = new HttpClient(fakeHandler);
            var client = new UserServiceClient(httpClient);

            // Act
            var result = await client.GetUsersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result![0].Username.Should().Be("alice");
        }

        [Fact]
        public async Task GetUsersAsync_WhenFailedStatus_ReturnsNull()
        {
            // Arrange
            var fakeHandler = new FakeHttpMessageHandler(req =>
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            });

            var httpClient = new HttpClient(fakeHandler);
            var client = new UserServiceClient(httpClient);

            // Act
            var result = await client.GetUsersAsync();

            // Assert
            result.Should().BeNull();
        }
    }
}
