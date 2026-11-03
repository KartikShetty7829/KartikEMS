using IdentityService.Entities;

namespace IdentityService.Interface
{
    public interface IUserRepository
    {
        Task<User?> GetAdminUserByUsernameAsync(string username, CancellationToken cancellationToken);
    }

}
