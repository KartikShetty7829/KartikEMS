using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Business_Handler;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EmployeeService.Tests.Handlers
{
    public class CreateDesignationsHandlerTests
    {
        [Fact]
        public async Task Handle_ValidRequest_BulkCreatesDesignations()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var designationRepo = DbContextHelper.CreateRepository<Designation>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<CreateDesignationsHandler>();

            var handler = new CreateDesignationsHandler(designationRepo, logger, unitOfWork);

            var request = new CreateDesignationsRequestDto
            {
                Designations = new List<SingleDesignationDto>
                {
                    new SingleDesignationDto { Title = "Junior Dev", Description = "Entry level" },
                    new SingleDesignationDto { Title = "Senior Dev", Description = "Senior level" }
                }
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("All designations created successfully");

            var saved = await context.Designations.ToListAsync();
            saved.Should().HaveCount(2);
            saved.Should().Contain(d => d.Title == "Junior Dev");
            saved.Should().Contain(d => d.Title == "Senior Dev");
        }
    }
}
