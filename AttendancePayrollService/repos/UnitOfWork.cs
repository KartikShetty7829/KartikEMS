using AttendancePayrollService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AttendancePayrollService.repos
{
    public class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext
    {
        private readonly TContext _context;

        public UnitOfWork(TContext context)
        {
            _context = context;

        }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        Task IUnitOfWork<TContext>.SaveChangesAsync()
        {
            return SaveChangesAsync();
        }
    }
}
