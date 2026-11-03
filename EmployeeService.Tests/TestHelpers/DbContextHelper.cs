using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace EmployeeService.Tests.TestHelpers
{
    public static class DbContextHelper
    {
        public static EmpDepartmentDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<EmpDepartmentDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            return new EmpDepartmentDbContext(options);
        }

        public static GenericRepository<EmpDepartmentDbContext, TEntity> CreateRepository<TEntity>(EmpDepartmentDbContext context)
            where TEntity : class
        {
            return new GenericRepository<EmpDepartmentDbContext, TEntity>(context);
        }

        public static UnitOfWork<EmpDepartmentDbContext> CreateUnitOfWork(EmpDepartmentDbContext context)
        {
            return new UnitOfWork<EmpDepartmentDbContext>(context);
        }

        public static ReadOnlyRepository<EmpDepartmentDbContext, TEntity> CreateReadOnlyRepository<TEntity>(EmpDepartmentDbContext context)
            where TEntity : class
        {
            return new ReadOnlyRepository<EmpDepartmentDbContext, TEntity>(context);
        }

        public static ILogger<T> CreateLogger<T>()
        {
            return new Mock<ILogger<T>>().Object;
        }
    }
}
