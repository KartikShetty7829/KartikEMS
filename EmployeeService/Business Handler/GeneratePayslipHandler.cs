using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Exceptions;
using EmployeeServiceDepartmentService.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeServiceDepartmentService.Business_Handler
{
    public class GeneratePayslipHandler : IRequestHandler<GeneratePayslipRequestDto, GeneratePayslipResponseDto>
    {
        private readonly EmpDepartmentDbContext _dbContext;
        private readonly IPayslipService _payslipService;
        private readonly ILogger<GeneratePayslipHandler> _logger;

        public GeneratePayslipHandler(EmpDepartmentDbContext dbContext, IPayslipService payslipService, ILogger<GeneratePayslipHandler> logger)
        {
            _dbContext = dbContext;
            _payslipService = payslipService;
            _logger = logger;
        }

        public async Task<GeneratePayslipResponseDto> Handle(GeneratePayslipRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate request
                if (request.EmployeeNumber <= 0)
                {
                    throw new ValidationException("Invalid employee number");
                }

                if (request.PayrollMonth < 1 || request.PayrollMonth > 12)
                {
                    throw new ValidationException("Invalid payroll month. Must be between 1 and 12");
                }

                if (request.PayrollYear < 2020 || request.PayrollYear > DateTime.Now.Year + 1)
                {
                    throw new ValidationException("Invalid payroll year");
                }

                _logger.LogInformation($"Generating payslip for employee {request.EmployeeNumber} for {request.PayrollMonth}/{request.PayrollYear}");

                // Get employee details
                var employee = await _dbContext.Employees
                    .FirstOrDefaultAsync(e => e.EmployeeNumber == request.EmployeeNumber, cancellationToken);

                if (employee == null)
                {
                    throw new NotFoundException($"Employee with number {request.EmployeeNumber} not found");
                }

                // Get employee designation (join EmployeeDesignation and Designation tables)
                var employeeDesignation = await _dbContext.EmployeeDesignations
                    .Where(ed => ed.EmployeeId == employee.EmployeeId && (ed.EndDate == null || ed.EndDate >= DateTime.Now))
                    .Include(ed => ed.Designation)
                    .FirstOrDefaultAsync(cancellationToken);

                var designationTitle = employeeDesignation?.Designation?.Title ?? "-";

                // Get payroll records
                var payrollRecord = await _dbContext.PayrollRecords
                    .FirstOrDefaultAsync(p => p.EmployeeId == employee.EmployeeId 
                        && p.PayrollMonth == request.PayrollMonth 
                        && p.PayrollYear == request.PayrollYear, cancellationToken);

                if (payrollRecord == null)
                {
                    throw new NotFoundException($"Payroll record not found for employee {request.EmployeeNumber} for {request.PayrollMonth}/{request.PayrollYear}");
                }

                // Generate PDF
                var employeeName = $"{employee.FirstName} {employee.LastName}";
                var pdfPath = await _payslipService.GeneratePayslipPdfAsync(employeeName, request.EmployeeNumber, payrollRecord, employee, designationTitle, cancellationToken);

                // Update PayrollRecord with PDF URL and generation timestamp
                payrollRecord.PayslipPdfUrl = pdfPath;
                payrollRecord.PayslipGeneratedDate = DateTime.Now;
                _dbContext.PayrollRecords.Update(payrollRecord);

                // Save payslip record (optional - for audit trail)
                var payslip = new Payslip
                {
                    PayslipId = Guid.NewGuid(),
                    EmployeeId = employee.EmployeeId,
                    PdfUrl = pdfPath
                };

                _dbContext.Payslips.Add(payslip);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation($"Payslip generated and saved successfully for employee {request.EmployeeNumber}. PDF URL stored in PayrollRecords table.");

                return new GeneratePayslipResponseDto
                {
                    Success = true,
                    Message = $"Payslip generated successfully for {employeeName}",
                    FilePath = pdfPath
                };
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning($"Not found: {ex.Message}");
                return new GeneratePayslipResponseDto
                {
                    Success = false,
                    Message = ex.Message,
                    FilePath = null
                };
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning($"Validation error: {ex.Message}");
                return new GeneratePayslipResponseDto
                {
                    Success = false,
                    Message = ex.Message,
                    FilePath = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating payslip: {ex.Message}");
                return new GeneratePayslipResponseDto
                {
                    Success = false,
                    Message = $"An error occurred while generating the payslip: {ex.Message}",
                    FilePath = null
                };
            }
        }
    }
}
