using EmployeeManagementSystem.Entities;
using EmployeeService.Business_Handler;
using EmployeeService.domains;
using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Entity;
using FluentAssertions;
using Xunit;

namespace EmployeeService.Tests.Handlers
{
    public class EmployeeProjectHandlerTests
    {
        [Fact]
        public async Task Handle_ValidRequest_ReturnsEmployeeProjects()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var empId = Guid.NewGuid();
            var projId = Guid.NewGuid();

            var employee = new Employees
            {
                EmployeeId = empId,
                EmployeeNumber = 7001,
                FirstName = "Peter",
                LastName = "Parker",
                Email = "peter@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            };
            var project = new Projects
            {
                ProjectId = projId,
                Name = "Web Platform",
                Description = "Internal web app",
                Status = "Active"
            };
            var empProject = new EmployeeProject
            {
                EmployeeProjectId = Guid.NewGuid(),
                EmployeeId = empId,
                Employee = employee,
                ProjectId = projId,
                Project = project
            };

            context.Employees.Add(employee);
            context.Projects.Add(project);
            context.EmployeeProjects.Add(empProject);
            await context.SaveChangesAsync();

            var employeeProjectRepo = DbContextHelper.CreateRepository<EmployeeProject>(context);
            var logger = DbContextHelper.CreateLogger<EmployeeProjectHandler>();

            var handler = new EmployeeProjectHandler(employeeProjectRepo, logger);

            var request = new EmployeeProjectRequestDto
            {
                EmployeeNumber = 7001
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.EmployeeNumber.Should().Be(7001);
            result.Projects.Should().HaveCount(1);
            result.Projects[0].Name.Should().Be("Web Platform");
        }

        [Fact]
        public async Task Handle_NoProjects_ReturnsEmptyList()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var employeeProjectRepo = DbContextHelper.CreateRepository<EmployeeProject>(context);
            var logger = DbContextHelper.CreateLogger<EmployeeProjectHandler>();

            var handler = new EmployeeProjectHandler(employeeProjectRepo, logger);

            var request = new EmployeeProjectRequestDto
            {
                EmployeeNumber = 7002
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Projects.Should().BeEmpty();
        }
    }
}
