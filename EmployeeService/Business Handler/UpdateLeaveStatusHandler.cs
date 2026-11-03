using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Exceptions;
using EmployeeServiceDepartmentService.interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeServiceDepartmentService.Business_Handler
{
    public class UpdateLeaveStatusHandler
        : IRequestHandler<UpdateLeaveStatusRequestDto, UpdateLeaveStatusResponseDto>
    {
        private readonly IGenericRepository<EmpDepartmentDbContext, LeaveRequests> _leaveRepo;
        private readonly IGenericRepository<EmpDepartmentDbContext, Employees> _employeeRepo;
        private readonly IUnitOfWork<EmpDepartmentDbContext> _unitOfWork;
        private readonly ILogger<UpdateLeaveStatusHandler> _logger;
        private readonly IKafkaProducerService _kafkaProducerService;

        public UpdateLeaveStatusHandler(
            IGenericRepository<EmpDepartmentDbContext, LeaveRequests> leaveRepo,
            IGenericRepository<EmpDepartmentDbContext, Employees> employeeRepo,
            IUnitOfWork<EmpDepartmentDbContext> unitOfWork,
            ILogger<UpdateLeaveStatusHandler> logger,
            IKafkaProducerService kafkaProducerService)
        {
            _leaveRepo = leaveRepo;
            _employeeRepo = employeeRepo;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _kafkaProducerService = kafkaProducerService;
        }

        public async Task<UpdateLeaveStatusResponseDto> Handle(
            UpdateLeaveStatusRequestDto request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Updating leave status for LeaveId: {LeaveId}",
                request.LeaveId);

            // Fetch Leave Request
            var leaveRequest = await _leaveRepo
                .Query()
                .FirstOrDefaultAsync(
                    l => l.LeaveId == request.LeaveId,
                    cancellationToken);

            if (leaveRequest == null)
            {
                throw new NotFoundException(
                    $"Leave request with ID {request.LeaveId} not found");
            }

            // Fetch Admin Employee using Employee Number
            var adminEmployee = await _employeeRepo
                .Query()
                .FirstOrDefaultAsync(
                    e => e.EmployeeNumber == request.AdminEmployeeNumber,
                    cancellationToken);

            if (adminEmployee == null)
            {
                throw new NotFoundException(
                    $"Employee with Employee Number {request.AdminEmployeeNumber} not found");
            }

            var action = request.Action.Trim().ToLower();

            if (action != "approved" && action != "rejected")
            {
                throw new BadRequestException(
                    "Invalid action. Must be 'Approved' or 'Rejected'.");
            }

            // Update Status
            leaveRequest.Status =
                action == "approved"
                    ? "Approved"
                    : "Rejected";

            // Store Admin EmployeeId in ApprovedBy
            leaveRequest.ApprovedBy = adminEmployee.EmployeeId;

            await _leaveRepo.UpdateAsync(leaveRequest);
            await _unitOfWork.SaveChangesAsync();

            // Produce Kafka Message for Leave Status Updated
            var kafkaMsg = $"{{\"Action\": \"Leave{leaveRequest.Status}\", \"LeaveId\": \"{leaveRequest.LeaveId}\", \"EmployeeId\": \"{leaveRequest.EmployeeId}\", \"Message\": \"Your {leaveRequest.Type} leave request from {leaveRequest.StartDate:dd-MMM-yyyy} to {leaveRequest.EndDate:dd-MMM-yyyy} was {leaveRequest.Status.ToLower()}\"}}";
            await _kafkaProducerService.ProduceAsync("leave-events", kafkaMsg);

            _logger.LogInformation(
                "Leave request {LeaveId} updated successfully by EmployeeNumber {EmployeeNumber}",
                request.LeaveId,
                request.AdminEmployeeNumber);

            return new UpdateLeaveStatusResponseDto
            {
                Success = true,
                Message = $"Leave request {leaveRequest.Status} successfully"
            };
        }
    }
}