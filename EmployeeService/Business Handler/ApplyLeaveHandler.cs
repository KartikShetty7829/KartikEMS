using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Exceptions;
using EmployeeServiceDepartmentService.interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeServiceDepartmentService.Business_Handler
{
    public class ApplyLeaveHandler
         : IRequestHandler<ApplyLeaveRequestDto, ApplyLeaveResponseDto>
    {
        private readonly IGenericRepository<EmpDepartmentDbContext, LeaveRequests> _leaveRepo;
        private readonly IGenericRepository<EmpDepartmentDbContext, Employees> _employeeRepo;
        private readonly IUnitOfWork<EmpDepartmentDbContext> _unitOfWork;
        private readonly ILogger<ApplyLeaveHandler> _logger;
        private readonly IEmailService _emailService;
        private readonly IMessageService _messageService;
        private readonly IKafkaProducerService _kafkaProducerService;

        public ApplyLeaveHandler(
            IGenericRepository<EmpDepartmentDbContext, LeaveRequests> leaveRepo,
            IGenericRepository<EmpDepartmentDbContext, Employees> employeeRepo,
            IUnitOfWork<EmpDepartmentDbContext> unitOfWork,
            ILogger<ApplyLeaveHandler> logger,
            IEmailService emailService,
            IMessageService messageService,
            IKafkaProducerService kafkaProducerService)
        {
            _leaveRepo = leaveRepo;
            _employeeRepo = employeeRepo;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _emailService = emailService;
            _messageService = messageService;
            _kafkaProducerService = kafkaProducerService;
        }

        public async Task<ApplyLeaveResponseDto> Handle(ApplyLeaveRequestDto request, CancellationToken cancellationToken)
        {
                // 1. Validate dates
                if (request.EndDate < request.StartDate)
                {
                    throw new BadRequestException("End date cannot be earlier than start date.");
                }

                // 2. Resolve EmployeeId from EmployeeNumber
                var employee = await _employeeRepo
                    .Query()
                    .FirstOrDefaultAsync(e => e.EmployeeNumber == request.EmployeeNumber, cancellationToken);

                if (employee == null)
                {
                    throw new NotFoundException("Employee not found. Cannot apply leave.");
                }

                // 3. Check for existing overlapping leave requests
                var existingLeave = await _leaveRepo
                    .Query()
                    .FirstOrDefaultAsync(l =>
                        l.EmployeeId == employee.EmployeeId &&
                        l.StartDate <= request.EndDate &&
                        l.EndDate >= request.StartDate,
                        cancellationToken);

                if (existingLeave != null)
                {
                    _logger.LogWarning("Duplicate leave request for employee {EmployeeNumber}", request.EmployeeNumber);
                    throw new BadRequestException("A leave request already exists for the given date range.");
                }

                // 4. Create Leave entity with resolved EmployeeId
                var leaveEntity = new LeaveRequests
                {
                    LeaveId = Guid.NewGuid(),
                    EmployeeId = employee.EmployeeId,
                    Type = request.Type,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Status = "Pending"
                };

                // 5. Save to DB
                await _leaveRepo.AddAsync(leaveEntity);
                await _unitOfWork.SaveChangesAsync();

                // Produce Kafka Message for Leave Applied
                var managerId = employee.ManagerId != null ? employee.ManagerId.ToString() : "";
                var kafkaMsg = $"{{\"Action\": \"LeaveApplied\", \"LeaveId\": \"{leaveEntity.LeaveId}\", \"EmployeeId\": \"{employee.EmployeeId}\", \"ManagerId\": \"{managerId}\", \"Message\": \"Employee {employee.FirstName} {employee.LastName} (Emp #: {employee.EmployeeNumber}) applied for {request.Type} leave from {request.StartDate:dd-MMM-yyyy} to {request.EndDate:dd-MMM-yyyy}\"}}";
                await _kafkaProducerService.ProduceAsync("leave-events", kafkaMsg);

                // 6. Send email notification via abstraction (DIP)
                if (!string.IsNullOrEmpty(employee.Email))
                {
                    try
                    {
                        string subject = "Leave Application Submitted";
                        string body = $"<h2>Hello {employee.FirstName} {employee.LastName},</h2>" +
                                      $"<p>Your leave request has been submitted successfully.</p>" +
                                      $"<p>Type: {request.Type}</p>" +
                                      $"<p>Start Date: {request.StartDate:dd-MMM-yyyy}</p>" +
                                      $"<p>End Date: {request.EndDate:dd-MMM-yyyy}</p>";

                        await _emailService.SendEmailAsync(employee.Email, subject, body);
                        _logger.LogInformation("Leave application email sent to {Email}", employee.Email);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send leave application email to {Email}", employee.Email);
                    }
                }

                // 7. Send SMS notification via abstraction (DIP)
                if (!string.IsNullOrEmpty(employee.Phone))
                {
                    try
                    {
                        string smsMessage = $"Hello {employee.FirstName} {employee.LastName}, your leave request ({request.Type}) " +
                                            $"from {request.StartDate:dd-MMM-yyyy} to {request.EndDate:dd-MMM-yyyy} has been submitted.";

                        await _messageService.SendSmsAsync(employee.Phone, smsMessage);
                        _logger.LogInformation("Leave application SMS sent to {PhoneNumber}", employee.Phone);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send leave application SMS to {PhoneNumber}", employee.Phone);
                    }
                }

                // 8. Return success with employee details
                return new ApplyLeaveResponseDto
                {
                    Success = true,
                    Message = "Leave request submitted successfully.",
                    Email = employee.Email,
                    Phone = employee.Phone,
                    EmployeeName = $"{employee.FirstName} {employee.LastName}"
                };
        }
    }
}
