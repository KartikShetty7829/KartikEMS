using EmployeeService.Tests.TestHelpers;
using EmployeeServiceDepartmentService.Business_Handler;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EmployeeService.Tests.Handlers
{
    public class AddEmployeeAddressHandlerTests
    {
        [Fact]
        public async Task Handle_ValidRequest_AddsAddressesSuccessfully()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var empId = Guid.NewGuid();
            context.Employees.Add(new Employees
            {
                EmployeeId = empId,
                EmployeeNumber = 1001,
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            await context.SaveChangesAsync();

            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var addressRepo = DbContextHelper.CreateRepository<EmployeeAddress>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<AddEmployeeAddressHandler>();

            var handler = new AddEmployeeAddressHandler(employeeRepo, addressRepo, unitOfWork, logger);

            var request = new AddEmployeeAddressesRequestDto
            {
                Employees = new List<EmployeeAddressesDto>
                {
                    new EmployeeAddressesDto
                    {
                        EmployeeNumber = 1001,
                        Addresses = new List<AddressDto>
                        {
                            new AddressDto
                            {
                                AddressLine1 = "123 Main St",
                                City = "Seattle",
                                State = "WA",
                                PostalCode = "98101",
                                Country = "USA",
                                AddressType = "Home"
                            }
                        }
                    }
                }
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            var savedAddresses = await context.employeeAddresses.Where(a => a.EmployeeId == empId).ToListAsync();
            savedAddresses.Should().HaveCount(1);
            savedAddresses[0].City.Should().Be("Seattle");
        }

        [Fact]
        public async Task Handle_EmployeeNotFound_SkipsAndCompletesSuccessfully()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var addressRepo = DbContextHelper.CreateRepository<EmployeeAddress>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<AddEmployeeAddressHandler>();

            var handler = new AddEmployeeAddressHandler(employeeRepo, addressRepo, unitOfWork, logger);

            var request = new AddEmployeeAddressesRequestDto
            {
                Employees = new List<EmployeeAddressesDto>
                {
                    new EmployeeAddressesDto
                    {
                        EmployeeNumber = 9999, // non-existent
                        Addresses = new List<AddressDto>
                        {
                            new AddressDto
                            {
                                AddressLine1 = "Unknown St",
                                City = "Nowhere",
                                State = "NA",
                                PostalCode = "00000",
                                Country = "NA",
                                AddressType = "Home"
                            }
                        }
                    }
                }
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            context.employeeAddresses.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_EmployeeAlreadyHasAddresses_SkipsInsert()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var empId = Guid.NewGuid();
            context.Employees.Add(new Employees
            {
                EmployeeId = empId,
                EmployeeNumber = 1002,
                FirstName = "Bob",
                LastName = "Jones",
                Email = "bob@example.com",
                Phone = "1234567890",
                HireDate = DateTime.UtcNow,
                Status = true
            });
            context.employeeAddresses.Add(new EmployeeAddress
            {
                AddressId = Guid.NewGuid(),
                EmployeeId = empId,
                AddressLine1 = "Existing Street",
                City = "Boston",
                State = "MA",
                PostalCode = "02108",
                Country = "USA",
                AddressType = "Home"
            });
            await context.SaveChangesAsync();

            var employeeRepo = DbContextHelper.CreateRepository<Employees>(context);
            var addressRepo = DbContextHelper.CreateRepository<EmployeeAddress>(context);
            var unitOfWork = DbContextHelper.CreateUnitOfWork(context);
            var logger = DbContextHelper.CreateLogger<AddEmployeeAddressHandler>();

            var handler = new AddEmployeeAddressHandler(employeeRepo, addressRepo, unitOfWork, logger);

            var request = new AddEmployeeAddressesRequestDto
            {
                Employees = new List<EmployeeAddressesDto>
                {
                    new EmployeeAddressesDto
                    {
                        EmployeeNumber = 1002,
                        Addresses = new List<AddressDto>
                        {
                            new AddressDto
                            {
                                AddressLine1 = "New Street",
                                City = "Chicago",
                                State = "IL",
                                PostalCode = "60601",
                                Country = "USA",
                                AddressType = "Home"
                            }
                        }
                    }
                }
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            var addresses = await context.employeeAddresses.Where(a => a.EmployeeId == empId).ToListAsync();
            addresses.Should().HaveCount(1);
            addresses[0].City.Should().Be("Boston");
        }
    }
}
