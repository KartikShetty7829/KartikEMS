using System;
using System.Threading.Tasks;
using AttendancePayrollService.externalservicereponse;

namespace AttendancePayrollService.Interfaces
{
    public interface IEmployeeServiceClient
    {
        Task<Guid?> GetEmployeeIdByNumberAsync(int employeeNumber);
        Task<EmployeeLookupDto?> GetEmployeeByIdAsync(Guid employeeId);

        Task<List<EmployeeLookupDto>> GetEmployeesByManagerIdAsync(Guid managerId);
    }
}
