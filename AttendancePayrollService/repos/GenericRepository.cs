using AttendancePayrollService.DataAccess;
using AttendancePayrollService.Entity;
using AttendancePayrollService.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AttendancePayrollService.repos
{
    public class GenericRepository<TContext, TEntity> : IGenericRepository<TContext, TEntity>
          where TContext : DbContext
          where TEntity : class

    {
        private readonly EmpAttendanceDbContext _context;
        private readonly DbSet<TEntity> _dbSet;


        public GenericRepository(EmpAttendanceDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        public async Task<TEntity> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);

        public async Task<IEnumerable<TEntity>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task AddAsync(TEntity entity) => await _dbSet.AddAsync(entity);


        public async Task UpdateAsync(TEntity entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TEntity entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        Task<TEntity> IGenericRepository<TContext, TEntity>.GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<TEntity>> IGenericRepository<TContext, TEntity>.GetAllAsync()
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<TEntity>> IGenericRepository<TContext, TEntity>.FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public IQueryable<AttendanceRecord> GetAttendanceRecord()
        {
            return _context.AttendanceRecords.AsQueryable();
        }
    }
}
