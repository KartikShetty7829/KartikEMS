using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EmployeeServiceDepartmentService.interfaces
{
    /// <summary>
    /// Read-only repository interface — contains ONLY generic read operations (LSP).
    /// Safe for read-only entities (e.g., Views) without throwing NotImplementedException on write methods.
    /// </summary>
    public interface IReadOnlyRepository<TContext, TEntity>
          where TContext : DbContext
          where TEntity : class
    {
        IQueryable<TEntity> Query();
        Task<TEntity> GetByIdAsync(Guid id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
    }
}
