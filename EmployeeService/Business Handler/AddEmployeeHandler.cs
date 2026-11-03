using EmployeeServiceDepartmentService.Common;
using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Exceptions;
using EmployeeServiceDepartmentService.interfaces;
using MediatR;

namespace EmployeeServiceDepartmentService.Business_Handler
{
    public class AddEmployeeHandler : IRequestHandler<EmployeeRequestDto, EmployeeResponseDto>
    {
        private readonly IGenericRepository<EmpDepartmentDbContext, Employees> _employeeRepo;
        private readonly ILogger<AddEmployeeHandler> _logger;
        private readonly IUnitOfWork<EmpDepartmentDbContext> _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IMessageService _messageService;

        public AddEmployeeHandler(
            IGenericRepository<EmpDepartmentDbContext, Employees> employeeRepo,
            ILogger<AddEmployeeHandler> logger,
            IUnitOfWork<EmpDepartmentDbContext> unitOfWork,
            IEmailService emailService,
            IMessageService messageService)
        {
            _employeeRepo = employeeRepo;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _messageService = messageService;
        }

        public async Task<EmployeeResponseDto> Handle(
            EmployeeRequestDto request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Starting add operation for employee {Email}",
                request.Email);

            // Check whether employee already exists
            var existing = _employeeRepo.Query()
                                        .FirstOrDefault(e => e.Email == request.Email);

            if (existing != null)
            {
                _logger.LogWarning(
                    "Employee with email {Email} already exists",
                    request.Email);

                throw new BadRequestException(
                    $"Employee with email '{request.Email}' already exists.");
            }

            // Create Employee
            var employee = new Employees
            {
                EmployeeId = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                HireDate = request.HireDate,
                Status = request.Status
            };

            await _employeeRepo.AddAsync(employee);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Employee {Email} created with Id {EmployeeId}",
                employee.Email,
                employee.EmployeeId);

            // Send Email
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                try
                {
                    string subject =
                        "Welcome to Kartik Software Solutions";

                    string body =
                        EmailTemplateService.GetWelcomeEmployeeTemplate(
                            request.FirstName,
                            request.LastName,
                            request.Email,
                            request.Phone,
                            request.HireDate);

                    await _emailService.SendEmailAsync(
                        request.Email,
                        subject,
                        body);

                    _logger.LogInformation(
                        "Notification email sent to {Email}",
                        request.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to send notification email to {Email}",
                        request.Email);
                }
            }

            // Send SMS
            if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                try
                {
                    string smsMessage =
                        $"Hello {request.FirstName} {request.LastName}, your employee record has been created successfully.";

                    await _messageService.SendSmsAsync(
                        request.Phone,
                        smsMessage);

                    _logger.LogInformation(
                        "SMS notification sent to {PhoneNumber}",
                        request.Phone);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to send SMS notification to {PhoneNumber}",
                        request.Phone);
                }
            }

            return new EmployeeResponseDto
            {
                EmployeeId = employee.EmployeeId,
                EmployeeNumber = employee.EmployeeNumber,
                Success = true,
                Message = $"Employee '{employee.FirstName} {employee.LastName}' added successfully."
            };
        }
    }
}