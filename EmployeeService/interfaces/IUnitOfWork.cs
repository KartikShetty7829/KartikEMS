using Microsoft.EntityFrameworkCore;

namespace EmployeeServiceDepartmentService.interfaces
{
    public interface IUnitOfWork<TContext> where TContext : DbContext
    {
        Task SaveChangesAsync();
    }
}
