using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeServiceDepartmentService.Business_Handler
{
    public class CreatePayrollRecordsHandler : IRequestHandler<CreatePayrollRecordsRequestDto, CreatePayrollRecordsResponseDto>
    {
        private readonly EmpDepartmentDbContext _dbContext;
        private readonly IGenericRepository<EmpDepartmentDbContext, PayrollRecords> _payrollRepo;
        private readonly ILogger<CreatePayrollRecordsHandler> _logger;

        public CreatePayrollRecordsHandler(
            EmpDepartmentDbContext dbContext,
            IGenericRepository<EmpDepartmentDbContext, PayrollRecords> payrollRepo,
            ILogger<CreatePayrollRecordsHandler> logger)
        {
            _dbContext = dbContext;
            _payrollRepo = payrollRepo;
            _logger = logger;
        }

        public async Task<CreatePayrollRecordsResponseDto> Handle(CreatePayrollRecordsRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Generating payroll records for Employee Number {request.EmployeeNumber} from {request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd}");

                // Validate employee exists
                var employee = await _dbContext.Employees
                    .FirstOrDefaultAsync(e => e.EmployeeNumber == request.EmployeeNumber, cancellationToken);

                if (employee == null)
                {
                    _logger.LogWarning($"Employee with Number {request.EmployeeNumber} not found");
                    return new CreatePayrollRecordsResponseDto
                    {
                        Success = false,
                        Message = "Employee not found"
                    };
                }

                // Validate date range
                if (request.StartDate > request.EndDate)
                {
                    _logger.LogWarning($"Invalid date range: StartDate {request.StartDate} is after EndDate {request.EndDate}");
                    return new CreatePayrollRecordsResponseDto
                    {
                        Success = false,
                        Message = "StartDate must be before or equal to EndDate"
                    };
                }

                // Call stored procedure: sp_GeneratePayroll if relational database provider is in use
                if (_dbContext.Database.IsRelational())
                {
                    await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                        $@"EXEC sp_GeneratePayroll 
                            @EmployeeNumber={request.EmployeeNumber},
                            @StartDate={request.StartDate},
                            @EndDate={request.EndDate}",
                        cancellationToken);
                }

                // Count how many payroll records were generated for this employee in the date range
                var generatedCount = await _dbContext.PayrollRecords
                    .CountAsync(p =>
                        p.EmployeeId == employee.EmployeeId &&
                        p.GeneratedDate >= request.StartDate &&
                        p.GeneratedDate <= request.EndDate.AddDays(1),
                        cancellationToken);

                _logger.LogInformation($"Payroll records generated successfully for Employee {request.EmployeeNumber}. Records: {generatedCount}");

                return new CreatePayrollRecordsResponseDto
                {
                    Success = true,
                    Message = "Payroll records generated successfully",
                    RecordsGenerated = generatedCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating payroll records: {ex.Message}");
                return new CreatePayrollRecordsResponseDto
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }
    }
}
