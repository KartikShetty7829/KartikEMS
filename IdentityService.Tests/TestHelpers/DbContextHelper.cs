using IdentityService.Data;
using IdentityService.Infrastructure;
using IdentityService.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace IdentityService.Tests.TestHelpers
{
    public static class DbContextHelper
    {
        public static IdentityDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            return new IdentityDbContext(options);
        }

        public static GenericRepository<DbContext, TEntity> CreateRepository<TEntity>(IdentityDbContext context)
            where TEntity : class
        {
            return new GenericRepository<DbContext, TEntity>(context);
        }

        public static UnitOfWork<IdentityDbContext> CreateUnitOfWork(IdentityDbContext context)
        {
            return new UnitOfWork<IdentityDbContext>(context);
        }

        public static UserRepository CreateUserRepository(IdentityDbContext context)
        {
            return new UserRepository(context);
        }

        public static ILogger<T> CreateLogger<T>()
        {
            return new Mock<ILogger<T>>().Object;
        }
    }
}
