using AttendancePayrollService.externalservicereponse;
using AttendancePayrollService.ExternalService;
using FluentAssertions;
using System.Net;
using System.Text.Json;
using Xunit;

namespace AttendancePayrollService.Tests.Services
{
    public class EmployeeServiceClientTests
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
        public async Task GetEmployeeIdByNumberAsync_WhenFound_ReturnsGuid()
        {
            // Arrange
            var expectedGuid = Guid.NewGuid();
            var employeeDto = new EmployeeLookupDto
            {
                EmployeeId = expectedGuid,
                EmployeeNumber = 1001,
                FullName = "John Doe"
            };

            var fakeHandler = new FakeHttpMessageHandler(req =>
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(employeeDto), System.Text.Encoding.UTF8, "application/json")
                };
            });

            var httpClient = new HttpClient(fakeHandler)
            {
                BaseAddress = new Uri("http://localhost:5000/")
            };

            var client = new EmployeeServiceClient(httpClient);

            // Act
            var result = await client.GetEmployeeIdByNumberAsync(1001);

            // Assert
            result.Should().Be(expectedGuid);
        }

        [Fact]
        public async Task GetEmployeeIdByNumberAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            var fakeHandler = new FakeHttpMessageHandler(req =>
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            var httpClient = new HttpClient(fakeHandler)
            {
                BaseAddress = new Uri("http://localhost:5000/")
            };

            var client = new EmployeeServiceClient(httpClient);

            // Act
            var result = await client.GetEmployeeIdByNumberAsync(9999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_WhenFound_ReturnsDto()
        {
            // Arrange
            var empId = Guid.NewGuid();
            var employeeDto = new EmployeeLookupDto
            {
                EmployeeId = empId,
                EmployeeNumber = 1002,
                FullName = "Jane Doe"
            };

            var fakeHandler = new FakeHttpMessageHandler(req =>
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(employeeDto), System.Text.Encoding.UTF8, "application/json")
                };
            });

            var httpClient = new HttpClient(fakeHandler)
            {
                BaseAddress = new Uri("http://localhost:5000/")
            };

            var client = new EmployeeServiceClient(httpClient);

            // Act
            var result = await client.GetEmployeeByIdAsync(empId);

            // Assert
            result.Should().NotBeNull();
            result!.EmployeeId.Should().Be(empId);
            result.FullName.Should().Be("Jane Doe");
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            var fakeHandler = new FakeHttpMessageHandler(req =>
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            var httpClient = new HttpClient(fakeHandler)
            {
                BaseAddress = new Uri("http://localhost:5000/")
            };

            var client = new EmployeeServiceClient(httpClient);

            // Act
            var result = await client.GetEmployeeByIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetEmployeesByManagerIdAsync_WhenSuccessful_ReturnsTeamList()
        {
            // Arrange
            var managerId = Guid.NewGuid();
            var team = new List<EmployeeLookupDto>
            {
                new EmployeeLookupDto { EmployeeId = Guid.NewGuid(), FullName = "Team 1", EmployeeNumber = 201 },
                new EmployeeLookupDto { EmployeeId = Guid.NewGuid(), FullName = "Team 2", EmployeeNumber = 202 }
            };

            var fakeHandler = new FakeHttpMessageHandler(req =>
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(team), System.Text.Encoding.UTF8, "application/json")
                };
            });

            var httpClient = new HttpClient(fakeHandler)
            {
                BaseAddress = new Uri("http://localhost:5000/")
            };

            var client = new EmployeeServiceClient(httpClient);

            // Act
            var result = await client.GetEmployeesByManagerIdAsync(managerId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].FullName.Should().Be("Team 1");
            result[1].FullName.Should().Be("Team 2");
        }
    }
}
