using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeServiceDepartmentService.Business_Handler
{
    public class GetPayrollRecordsHandler : IRequestHandler<GetPayrollRecordsRequestDto, GetPayrollRecordsResponseDto>
    {
        private readonly EmpDepartmentDbContext _dbContext;
        private readonly ILogger<GetPayrollRecordsHandler> _logger;

        public GetPayrollRecordsHandler(
            EmpDepartmentDbContext dbContext,
            ILogger<GetPayrollRecordsHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<GetPayrollRecordsResponseDto> Handle(GetPayrollRecordsRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Fetching payroll records for Employee Number {request.EmployeeNumber}");

                // Get employee
                var employee = await _dbContext.Employees
                    .FirstOrDefaultAsync(e => e.EmployeeNumber == request.EmployeeNumber, cancellationToken);

                if (employee == null)
                {
                    _logger.LogWarning($"Employee with Number {request.EmployeeNumber} not found");
                    return new GetPayrollRecordsResponseDto
                    {
                        Success = false,
                        Message = "Employee not found"
                    };
                }

                // Get all payroll records for this employee
                var payrollRecords = await _dbContext.PayrollRecords
                    .Where(p => p.EmployeeId == employee.EmployeeId)
                    .OrderByDescending(p => p.PayrollYear)
                    .ThenByDescending(p => p.PayrollMonth)
                    .ToListAsync(cancellationToken);

                if (!payrollRecords.Any())
                {
                    _logger.LogInformation($"No payroll records found for Employee {request.EmployeeNumber}");
                    return new GetPayrollRecordsResponseDto
                    {
                        Success = true,
                        Message = "No payroll records found for this employee",
                        PayrollRecords = new List<PayrollRecordDto>()
                    };
                }

                // Map to DTOs
                var payrollDtos = payrollRecords.Select(p => new PayrollRecordDto
                {
                    PayrollRecordId = p.PayrollRecordId,
                    EmployeeNumber = employee.EmployeeNumber,
                    EmployeeName = $"{employee.FirstName} {employee.LastName}",
                    PayrollMonth = p.PayrollMonth,
                    PayrollYear = p.PayrollYear,
                    BaseSalary = p.BaseSalary,
                    Allowances = p.Allowances,
                    Deductions = p.Deductions,
                    NetPay = p.NetPay,
                    WorkingDays = p.WorkingDays ?? 0,
                    PresentDays = p.PresentDays ?? 0,
                    GeneratedDate = p.GeneratedDate
                }).ToList();

                _logger.LogInformation($"Retrieved {payrollDtos.Count} payroll records for Employee {request.EmployeeNumber}");

                return new GetPayrollRecordsResponseDto
                {
                    Success = true,
                    Message = "Payroll records retrieved successfully",
                    PayrollRecords = payrollDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching payroll records: {ex.Message}");
                return new GetPayrollRecordsResponseDto
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }
    }
}
