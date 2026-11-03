using EmployeeServiceDepartmentService.interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EmployeeServiceDepartmentService.repos
{
    public class ReadOnlyRepository<TContext, TEntity> : IReadOnlyRepository<TContext, TEntity>
         where TContext : DbContext
         where TEntity : class
    {
        protected readonly TContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public ReadOnlyRepository(TContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        public IQueryable<TEntity> Query()
        {
            return _dbSet.AsQueryable();
        }

        public async Task<TEntity> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
    }
}
