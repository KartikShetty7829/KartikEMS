using IdentityService.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Interface
{
    public interface IUnitOfWork<TContext> where TContext : DbContext
    {
       

        Task SaveChangesAsync();
    }
}

