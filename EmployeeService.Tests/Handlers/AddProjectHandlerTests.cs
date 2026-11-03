using EmployeeManagementSystem.Entities;
using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Business_Handler;
using EmployeeServiceDepartmentService.domains;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EmployeeService.Tests.Handlers
{
    public class AddProjectHandlerTests
    {
        [Fact]
        public async Task Handle_ValidRequest_CreatesProjectSuccessfully()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var projectRepo = DbContextHelper.CreateRepository<Projects>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<AddProjectHandler>();

            var handler = new AddProjectHandler(projectRepo, unitOfWork, logger);

            var request = new AddProjectRequestDto
            {
                Name = "Project Alpha",
                Description = "A core platform project",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddMonths(6),
                Status = "Active"
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Project created successfully");

            var project = await context.Projects.FirstOrDefaultAsync(p => p.Name == "Project Alpha");
            project.Should().NotBeNull();
            project!.Description.Should().Be("A core platform project");
            project.Status.Should().Be("Active");
        }
    }
}
