using AttendancePayrollService.DataAccess;
using AttendancePayrollService.repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace AttendancePayrollService.Tests.TestHelpers
{
    public static class DbContextHelper
    {
        public static EmpAttendanceDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<EmpAttendanceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            return new EmpAttendanceDbContext(options);
        }

        public static GenericRepository<EmpAttendanceDbContext, TEntity> CreateRepository<TEntity>(EmpAttendanceDbContext context)
            where TEntity : class
        {
            return new GenericRepository<EmpAttendanceDbContext, TEntity>(context);
        }

        public static UnitOfWork<EmpAttendanceDbContext> CreateUnitOfWork(EmpAttendanceDbContext context)
        {
            return new UnitOfWork<EmpAttendanceDbContext>(context);
        }

        public static ILogger<T> CreateLogger<T>()
        {
            return new Mock<ILogger<T>>().Object;
        }
    }
}
