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
    public class UpdateEmployeeProfileHandlerTests
    {
        [Fact]
        public async Task Handle_ValidRequest_UpdatesProfileAndExistingAddress()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var empId = Guid.NewGuid();

            context.Employees.Add(new Employees
            {
                EmployeeId = empId,
                EmployeeNumber = 17001,
                FirstName = "Vision",
                LastName = "Synth",
                Email = "vision@example.com",
                Phone = "1231231234",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            context.employeeAddresses.Add(new EmployeeAddress
            {
                AddressId = Guid.NewGuid(),
                EmployeeId = empId,
                AddressLine1 = "Old Street",
                City = "London",
                State = "England",
                PostalCode = "EC1",
                Country = "UK",
                AddressType = "Home"
            });
            await context.SaveChangesAsync();

            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var addressRepo = DbContextHelper.CreateRepository<EmployeeAddress>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<UpdateEmployeeProfileHandler>();

            var handler = new UpdateEmployeeProfileHandler(employeeRepo, addressRepo, logger, unitOfWork);

            var request = new UpdateEmployeeProfileRequestDto
            {
                EmployeeNumber = 17001,
                Email = "vision.new@example.com",
                Phone = "9879879870",
                AddressLine1 = "New Road",
                AddressLine2 = "Apt 2B",
                City = "Manchester",
                State = "Lancashire",
                PostalCode = "M1",
                Country = "UK",
                AddressType = "Residential"
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            var employee = await context.Employees.FirstOrDefaultAsync(e => e.EmployeeNumber == 17001);
            employee!.Email.Should().Be("vision.new@example.com");
            employee.Phone.Should().Be("9879879870");

            var address = await context.employeeAddresses.FirstOrDefaultAsync(a => a.EmployeeId == empId);
            address.Should().NotBeNull();
            address!.AddressLine1.Should().Be("New Road");
            address.City.Should().Be("Manchester");
        }

        [Fact]
        public async Task Handle_ValidRequest_CreatesNewAddressWhenNoneExists()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var empId = Guid.NewGuid();

            context.Employees.Add(new Employees
            {
                EmployeeId = empId,
                EmployeeNumber = 17002,
                FirstName = "Clint",
                LastName = "Barton",
                Email = "clint@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            await context.SaveChangesAsync();

            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var addressRepo = DbContextHelper.CreateRepository<EmployeeAddress>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<UpdateEmployeeProfileHandler>();

            var handler = new UpdateEmployeeProfileHandler(employeeRepo, addressRepo, logger, unitOfWork);

            var request = new UpdateEmployeeProfileRequestDto
            {
                EmployeeNumber = 17002,
                Email = "clint.updated@example.com",
                Phone = "5554443333",
                AddressLine1 = "Farmhouse Lane",
                City = "Waverly",
                State = "Iowa",
                PostalCode = "50677",
                Country = "USA",
                AddressType = "Home"
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();

            var address = await context.employeeAddresses.FirstOrDefaultAsync(a => a.EmployeeId == empId);
            address.Should().NotBeNull();
            address!.AddressLine1.Should().Be("Farmhouse Lane");
            address.City.Should().Be("Waverly");
        }

        [Fact]
        public async Task Handle_EmployeeNotFound_ThrowsNotFoundException()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var addressRepo = DbContextHelper.CreateRepository<EmployeeAddress>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<UpdateEmployeeProfileHandler>();

            var handler = new UpdateEmployeeProfileHandler(employeeRepo, addressRepo, logger, unitOfWork);

            var request = new UpdateEmployeeProfileRequestDto
            {
                EmployeeNumber = 99999
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*Employee not found*");
        }
    }
}
