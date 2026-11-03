using EmployeeServiceDepartmentService.ExternalServiceResponse;

namespace EmployeeServiceDepartmentService.interfaces
{
    /// <summary>
    /// Abstraction for the external UserService HTTP client — DIP compliance.
    /// </summary>
    public interface IUserServiceClient
    {
        Task<List<GetUsersResponseDto>?> GetUsersAsync();
    }
}
