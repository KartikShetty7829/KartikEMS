using EmployeeServiceDepartmentService.interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EmployeeServiceDepartmentService.repos
{
    /// <summary>
    /// Generic repository — contains ONLY generic CRUD operations.
    /// Uses TContext properly (DIP) instead of concrete EmpDepartmentDbContext.
    /// No entity-specific methods — consumers use Query() with LINQ (ISP + OCP).
    /// </summary>
    public class GenericRepository<TContext, TEntity> : ReadOnlyRepository<TContext, TEntity>, IGenericRepository<TContext, TEntity>
         where TContext : DbContext
         where TEntity : class
    {
        public GenericRepository(TContext context) : base(context)
        {
        }

        public async Task AddAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _dbSet.AddAsync(entity);
        }

        public Task UpdateAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }
    }
}
