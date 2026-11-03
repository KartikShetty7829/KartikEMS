using AttendancePayrollService.externalservicereponse;
using AttendancePayrollService.Interfaces;

namespace AttendancePayrollService.ExternalService
{
    public class EmployeeServiceClient : IEmployeeServiceClient
    {
      
            private readonly HttpClient _httpClient;

            public EmployeeServiceClient(HttpClient httpClient)
            {
                _httpClient = httpClient;
            }
        //Common Methods to get the data from other Service 
        public async Task<Guid?> GetEmployeeIdByNumberAsync(int employeeNumber)
        {
            var response = await _httpClient.GetAsync(
                $"api/v1/Employee/employeeNumber?employeeNumber={employeeNumber}");

            if (!response.IsSuccessStatusCode)
                return null;

            var employee = await response.Content
                .ReadFromJsonAsync<EmployeeLookupDto>();

            return employee?.EmployeeId;
        }

        // ✅ New method: fetch employee details by ID
        public async Task<EmployeeLookupDto?> GetEmployeeByIdAsync(Guid employeeId)
        {
            // 👈 Use slash, not equals
            var response = await _httpClient.GetAsync($"api/v1/Employee/getemployeebyid/{employeeId}");
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<EmployeeLookupDto>();
        }

        public async Task<List<EmployeeLookupDto>> GetEmployeesByManagerIdAsync(Guid managerId)
        {
            var response = await _httpClient.GetAsync(
                $"api/v1/Employee/team-members?managerId={managerId}");

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<List<EmployeeLookupDto>>();
        }
    }
}

