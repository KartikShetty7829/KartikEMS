using IdentityService.Data;
using IdentityService.Entities;
using IdentityService.Interface;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IdentityDbContext _context;

        public UserRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetAdminUserByUsernameAsync(string username, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Include(u => u.Role) // eager load Role
                .Where(u => u.Username == username
                         && u.Role.Name == "Admin") // filter on Role.Name
                .FirstOrDefaultAsync(cancellationToken);
        }


    }

}
