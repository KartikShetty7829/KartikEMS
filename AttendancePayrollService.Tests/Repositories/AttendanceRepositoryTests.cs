using AttendancePayrollService.Entity;
using AttendancePayrollService.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AttendancePayrollService.Tests.Repositories
{
    public class AttendanceRepositoryTests
    {
        [Fact]
        public async Task AddAsync_AddsAttendanceRecordSuccessfully()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var repo = DbContextHelper.CreateRepository<AttendanceRecord>(context);
            var record = new AttendanceRecord
            {
                RecordId = Guid.NewGuid(),
                EmployeeId = Guid.NewGuid(),
                CheckInAt = DateTime.UtcNow,
                Status = "Pending"
            };

            // Act
            await repo.AddAsync(record);
            await context.SaveChangesAsync();

            // Assert
            var saved = await context.AttendanceRecords.FindAsync(record.RecordId);
            saved.Should().NotBeNull();
            saved!.EmployeeId.Should().Be(record.EmployeeId);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsExistingRecord()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var recordId = Guid.NewGuid();
            var record = new AttendanceRecord
            {
                RecordId = recordId,
                EmployeeId = Guid.NewGuid(),
                CheckInAt = DateTime.UtcNow,
                Status = "Pending"
            };
            context.AttendanceRecords.Add(record);
            await context.SaveChangesAsync();

            var repo = DbContextHelper.CreateRepository<AttendanceRecord>(context);

            // Act
            var result = await repo.GetByIdAsync(recordId);

            // Assert
            result.Should().NotBeNull();
            result.RecordId.Should().Be(recordId);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllRecords()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            context.AttendanceRecords.AddRange(
                new AttendanceRecord { RecordId = Guid.NewGuid(), EmployeeId = Guid.NewGuid(), CheckInAt = DateTime.UtcNow },
                new AttendanceRecord { RecordId = Guid.NewGuid(), EmployeeId = Guid.NewGuid(), CheckInAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            var repo = DbContextHelper.CreateRepository<AttendanceRecord>(context);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesRecordAndPersists()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var record = new AttendanceRecord
            {
                RecordId = Guid.NewGuid(),
                EmployeeId = Guid.NewGuid(),
                CheckInAt = DateTime.UtcNow,
                Status = "Pending"
            };
            context.AttendanceRecords.Add(record);
            await context.SaveChangesAsync();

            var repo = DbContextHelper.CreateRepository<AttendanceRecord>(context);

            // Act
            record.Status = "Approved";
            record.ApprovedBy = "Manager";
            await repo.UpdateAsync(record);

            // Assert
            var updated = await context.AttendanceRecords.FindAsync(record.RecordId);
            updated!.Status.Should().Be("Approved");
            updated.ApprovedBy.Should().Be("Manager");
        }

        [Fact]
        public async Task DeleteAsync_RemovesRecordAndPersists()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var record = new AttendanceRecord
            {
                RecordId = Guid.NewGuid(),
                EmployeeId = Guid.NewGuid(),
                CheckInAt = DateTime.UtcNow
            };
            context.AttendanceRecords.Add(record);
            await context.SaveChangesAsync();

            var repo = DbContextHelper.CreateRepository<AttendanceRecord>(context);

            // Act
            await repo.DeleteAsync(record);

            // Assert
            var deleted = await context.AttendanceRecords.FindAsync(record.RecordId);
            deleted.Should().BeNull();
        }

        [Fact]
        public void GetAttendanceRecord_ReturnsQueryable()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var empId = Guid.NewGuid();
            context.AttendanceRecords.Add(new AttendanceRecord
            {
                RecordId = Guid.NewGuid(),
                EmployeeId = empId,
                CheckInAt = DateTime.UtcNow,
                Status = "Pending"
            });
            context.SaveChanges();

            var repo = DbContextHelper.CreateRepository<AttendanceRecord>(context);

            // Act
            var query = repo.GetAttendanceRecord();

            // Assert
            query.Should().NotBeNull();
            query.Where(a => a.EmployeeId == empId).Count().Should().Be(1);
        }

        [Fact]
        public async Task UnitOfWork_SaveChangesAsync_PersistsChangesSuccessfully()
        {
            // Arrange
            using var context = DbContextHelper.CreateInMemoryDbContext();
            var uow = DbContextHelper.CreateUnitOfWork(context);

            context.AttendanceRecords.Add(new AttendanceRecord
            {
                RecordId = Guid.NewGuid(),
                EmployeeId = Guid.NewGuid(),
                CheckInAt = DateTime.UtcNow
            });

            // Act
            var savedCount = await uow.SaveChangesAsync();

            // Assert
            savedCount.Should().Be(1);
            context.AttendanceRecords.Count().Should().Be(1);
        }
    }
}
