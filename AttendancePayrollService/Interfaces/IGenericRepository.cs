using AttendancePayrollService.Entity;
//using EmployeeServiceDepartmentService.Entity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AttendancePayrollService.Interfaces
{
    public interface IGenericRepository<TContext, TEntity>
          where TContext : DbContext
          where TEntity : class
    {
        Task<TEntity> GetByIdAsync(Guid id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TEntity entity);

        IQueryable<AttendanceRecord> GetAttendanceRecord();

        //IQueryable<Employees> GetEmployees();
        //Task UpdateHireDateAsync(Employees employee);
    }
}
