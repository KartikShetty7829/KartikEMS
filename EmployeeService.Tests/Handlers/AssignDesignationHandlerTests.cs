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
    public class AssignDesignationHandlerTests
    {
        [Fact]
        public async Task Handle_ValidRequest_AssignsDesignationSuccessfully()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var empId = Guid.NewGuid();
            var desigId = Guid.NewGuid();

            context.Employees.Add(new Employees
            {
                EmployeeId = empId,
                EmployeeNumber = 3001,
                FirstName = "George",
                LastName = "Clark",
                Email = "george@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            context.Designations.Add(new Designation
            {
                DesignationId = desigId,
                Title = "Software Engineer",
                Description = "Developer role"
            });
            await context.SaveChangesAsync();

            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var designationRepo = DbContextHelper.CreateRepository<Designation>(context);
            var employeeDesignationRepo = DbContextHelper.CreateRepository<EmployeeDesignation>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<AssignDesignationHandler>();

            var handler = new AssignDesignationHandler(
                employeeRepo, designationRepo, employeeDesignationRepo, unitOfWork, logger);

            var request = new AssignDesignationRequestDto
            {
                EmployeeNumber = 3001,
                DesignationName = "Software Engineer",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddYears(1)
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            var assigned = await context.EmployeeDesignations.FirstOrDefaultAsync(ed => ed.EmployeeId == empId);
            assigned.Should().NotBeNull();
            assigned!.DesignationId.Should().Be(desigId);
        }

        [Fact]
        public async Task Handle_EmployeeNotFound_ThrowsNotFoundException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var designationRepo = DbContextHelper.CreateRepository<Designation>(context);
            var employeeDesignationRepo = DbContextHelper.CreateRepository<EmployeeDesignation>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<AssignDesignationHandler>();

            var handler = new AssignDesignationHandler(
                employeeRepo, designationRepo, employeeDesignationRepo, unitOfWork, logger);

            var request = new AssignDesignationRequestDto
            {
                EmployeeNumber = 9999,
                DesignationName = "Software Engineer",
                StartDate = DateTime.UtcNow.Date
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*Employee with number 9999 not found*");
        }

        [Fact]
        public async Task Handle_DesignationNotFound_ThrowsNotFoundException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            context.Employees.Add(new Employees
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeNumber = 3002,
                FirstName = "Hannah",
                LastName = "White",
                Email = "hannah@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            await context.SaveChangesAsync();

            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var designationRepo = DbContextHelper.CreateRepository<Designation>(context);
            var employeeDesignationRepo = DbContextHelper.CreateRepository<EmployeeDesignation>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<AssignDesignationHandler>();

            var handler = new AssignDesignationHandler(
                employeeRepo, designationRepo, employeeDesignationRepo, unitOfWork, logger);

            var request = new AssignDesignationRequestDto
            {
                EmployeeNumber = 3002,
                DesignationName = "NonExistentDesignation",
                StartDate = DateTime.UtcNow.Date
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*Designation 'NonExistentDesignation' not found*");
        }

        [Fact]
        public async Task Handle_AlreadyAssigned_ThrowsBadRequestException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var empId = Guid.NewGuid();
            var desigId = Guid.NewGuid();

            context.Employees.Add(new Employees
            {
                EmployeeId = empId,
                EmployeeNumber = 3003,
                FirstName = "Ian",
                LastName = "Wright",
                Email = "ian@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            context.Designations.Add(new Designation
            {
                DesignationId = desigId,
                Title = "Lead Architect"
            });
            context.EmployeeDesignations.Add(new EmployeeDesignation
            {
                EmployeeDesignationId = Guid.NewGuid(),
                EmployeeId = empId,
                DesignationId = desigId,
                StartDate = DateTime.UtcNow.Date.AddDays(-10),
                EndDate = null
            });
            await context.SaveChangesAsync();

            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var designationRepo = DbContextHelper.CreateRepository<Designation>(context);
            var employeeDesignationRepo = DbContextHelper.CreateRepository<EmployeeDesignation>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<AssignDesignationHandler>();

            var handler = new AssignDesignationHandler(
                employeeRepo, designationRepo, employeeDesignationRepo, unitOfWork, logger);

            var request = new AssignDesignationRequestDto
            {
                EmployeeNumber = 3003,
                DesignationName = "Lead Architect",
                StartDate = DateTime.UtcNow.Date
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("*already has designation 'Lead Architect' assigned*");
        }
    }
}
