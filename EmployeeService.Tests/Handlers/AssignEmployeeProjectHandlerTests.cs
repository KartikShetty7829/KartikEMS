using EmployeeManagementSystem.Entities;
using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Business_Handler;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EmployeeService.Tests.Handlers
{
    public class AssignEmployeeProjectHandlerTests
    {
        [Fact]
        public async Task Handle_ValidRequest_AssignsProjectSuccessfully()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var empId = Guid.NewGuid();
            var projId = Guid.NewGuid();
            var desigId = Guid.NewGuid();

            context.Employees.Add(new Employees
            {
                EmployeeId = empId,
                EmployeeNumber = 4001,
                FirstName = "Jack",
                LastName = "Brown",
                Email = "jack@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            context.Projects.Add(new Projects
            {
                ProjectId = projId,
                Name = "Project Beta",
                Description = "Beta version",
                Status = "Active"
            });
            context.Designations.Add(new Designation
            {
                DesignationId = desigId,
                Title = "Full Stack Engineer"
            });
            await context.SaveChangesAsync();

            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var projectRepo = DbContextHelper.CreateRepository<Projects>(context);
            var designationRepo = DbContextHelper.CreateRepository<Designation>(context);
            var employeeProjectRepo = DbContextHelper.CreateRepository<EmployeeProject>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<AssignEmployeeProjectHandler>();

            var handler = new AssignEmployeeProjectHandler(
                employeeRepo, projectRepo, designationRepo, employeeProjectRepo, unitOfWork, logger);

            var request = new AssignEmployeeProjectRequestDto
            {
                EmployeeNumber = 4001,
                ProjectName = "Project Beta",
                DesignationTitle = "Full Stack Engineer",
                AssignedDate = DateTime.UtcNow.Date,
                ReleaseDate = DateTime.UtcNow.Date.AddMonths(3)
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            var assigned = await context.EmployeeProjects.FirstOrDefaultAsync(ep => ep.EmployeeId == empId);
            assigned.Should().NotBeNull();
            assigned!.ProjectId.Should().Be(projId);
            assigned.DesignationId.Should().Be(desigId);
        }

        [Fact]
        public async Task Handle_EmployeeNotFound_ThrowsNotFoundException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var projectRepo = DbContextHelper.CreateRepository<Projects>(context);
            var designationRepo = DbContextHelper.CreateRepository<Designation>(context);
            var employeeProjectRepo = DbContextHelper.CreateRepository<EmployeeProject>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<AssignEmployeeProjectHandler>();

            var handler = new AssignEmployeeProjectHandler(
                employeeRepo, projectRepo, designationRepo, employeeProjectRepo, unitOfWork, logger);

            var request = new AssignEmployeeProjectRequestDto
            {
                EmployeeNumber = 9999,
                ProjectName = "Project Beta",
                DesignationTitle = "Lead"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*Employee with number 9999 not found*");
        }

        [Fact]
        public async Task Handle_ProjectNotFound_ThrowsNotFoundException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            context.Employees.Add(new Employees
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeNumber = 4002,
                FirstName = "Karen",
                LastName = "Page",
                Email = "karen@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            await context.SaveChangesAsync();

            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var projectRepo = DbContextHelper.CreateRepository<Projects>(context);
            var designationRepo = DbContextHelper.CreateRepository<Designation>(context);
            var employeeProjectRepo = DbContextHelper.CreateRepository<EmployeeProject>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<AssignEmployeeProjectHandler>();

            var handler = new AssignEmployeeProjectHandler(
                employeeRepo, projectRepo, designationRepo, employeeProjectRepo, unitOfWork, logger);

            var request = new AssignEmployeeProjectRequestDto
            {
                EmployeeNumber = 4002,
                ProjectName = "NonExistentProject",
                DesignationTitle = "Lead"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*Project with name NonExistentProject not found*");
        }

        [Fact]
        public async Task Handle_DesignationNotFound_ThrowsNotFoundException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            context.Employees.Add(new Employees
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeNumber = 4003,
                FirstName = "Luke",
                LastName = "Cage",
                Email = "luke@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            context.Projects.Add(new Projects
            {
                ProjectId = Guid.NewGuid(),
                Name = "Project Gamma",
                Status = "Active"
            });
            await context.SaveChangesAsync();

            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var projectRepo = DbContextHelper.CreateRepository<Projects>(context);
            var designationRepo = DbContextHelper.CreateRepository<Designation>(context);
            var employeeProjectRepo = DbContextHelper.CreateRepository<EmployeeProject>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<AssignEmployeeProjectHandler>();

            var handler = new AssignEmployeeProjectHandler(
                employeeRepo, projectRepo, designationRepo, employeeProjectRepo, unitOfWork, logger);

            var request = new AssignEmployeeProjectRequestDto
            {
                EmployeeNumber = 4003,
                ProjectName = "Project Gamma",
                DesignationTitle = "NonExistentDesignation"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*Designation with title NonExistentDesignation not found*");
        }
    }
}
