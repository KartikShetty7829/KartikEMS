using IdentityService.Data;
using IdentityService.Entities;
using IdentityService.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
//using static IdentityService.Interface.IGenericRepository;


namespace IdentityService.Repositories
{
    public class GenericRepository<TContext, TEntity> : IGenericRepository<TContext, TEntity>
        where TContext : DbContext
        where TEntity : class
       
    {
        private readonly IdentityDbContext _context;
        private readonly DbSet<TEntity> _dbSet;

        public GenericRepository(IdentityDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        public async Task<TEntity> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);

        public async Task<IEnumerable<TEntity>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task AddAsync(TEntity entity)=>await _dbSet.AddAsync(entity);
     

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

        
        IQueryable<User> IGenericRepository<TContext, TEntity>.GetUsers()
        {
            return _context.Set<User>();
        }

        IQueryable<Role> IGenericRepository<TContext, TEntity>.GetRoles()
        {
            return _context.Set<Role>();
        }
    }
}
