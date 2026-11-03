using Microsoft.EntityFrameworkCore;

namespace AttendancePayrollService.Interfaces
{
    public interface IUnitOfWork<TContext> where TContext : DbContext
    {
        Task SaveChangesAsync();
    }
}
