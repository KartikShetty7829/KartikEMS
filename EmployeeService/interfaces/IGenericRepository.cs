using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EmployeeServiceDepartmentService.interfaces
{
    /// <summary>
    /// Generic repository interface — contains ONLY generic CRUD operations.
    /// Entity-specific queries use Query() with LINQ — no modifications needed for new entities (OCP).
    /// </summary>
    public interface IGenericRepository<TContext, TEntity> : IReadOnlyRepository<TContext, TEntity>
          where TContext : DbContext
          where TEntity : class
    {
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TEntity entity);
    }
}
