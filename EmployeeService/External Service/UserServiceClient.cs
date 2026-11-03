using EmployeeServiceDepartmentService.ExternalServiceResponse;
using EmployeeServiceDepartmentService.interfaces;

namespace EmployeeServiceDepartmentService.External_Service
{
    /// <summary>
    /// HTTP client for the external UserService — implements IUserServiceClient (DIP).
    /// </summary>
    public class UserServiceClient : IUserServiceClient
    {
        private readonly HttpClient _httpClient;

        public UserServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<GetUsersResponseDto>?> GetUsersAsync()
        {
            var response = await _httpClient.GetAsync("http://localhost:5039/api/Identity/getusers");

            if (!response.IsSuccessStatusCode)
                return null;

            var envelope = await response.Content.ReadFromJsonAsync<GetUsersEnvelope>();

            return envelope?.Users;
        }
    }
}
