using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmployeeServiceDepartmentService.Business_Handler
{
    public class GetEmployeesWithPendingLeavesHandler
        : IRequestHandler<GetEmployeesWithPendingLeavesRequestDto,
            GetEmployeesWithPendingLeavesResponseDto>
    {
        private readonly IGenericRepository<EmpDepartmentDbContext, LeaveRequests> _leaveRepo;
        private readonly IGenericRepository<EmpDepartmentDbContext, Employees> _employeeRepo;
        private readonly ILogger<GetEmployeesWithPendingLeavesHandler> _logger;

        public GetEmployeesWithPendingLeavesHandler(
            IGenericRepository<EmpDepartmentDbContext, LeaveRequests> leaveRepo,
            IGenericRepository<EmpDepartmentDbContext, Employees> employeeRepo,
            ILogger<GetEmployeesWithPendingLeavesHandler> logger)
        {
            _leaveRepo = leaveRepo;
            _employeeRepo = employeeRepo;
            _logger = logger;
        }

        public async Task<GetEmployeesWithPendingLeavesResponseDto> Handle(
            GetEmployeesWithPendingLeavesRequestDto request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Fetching pending leave requests for Employee Number : {EmployeeNumber}",
                request.EmployeeNumber);

            // Step 1 : Get Manager Employee Record
            var manager = await _employeeRepo.Query()
                .FirstOrDefaultAsync(
                    x => x.EmployeeNumber == request.EmployeeNumber,
                    cancellationToken);

            if (manager == null)
            {
                return new GetEmployeesWithPendingLeavesResponseDto
                {
                    PendingLeaves = new List<EmployeePendingLeaveDto>()
                };
            }

            // Step 2 : Get Employees Reporting To This Manager
            var teamMembers = await _employeeRepo.Query()
                .Where(x => x.ManagerId == manager.EmployeeId)
                .ToListAsync(cancellationToken);

            if (!teamMembers.Any())
            {
                return new GetEmployeesWithPendingLeavesResponseDto
                {
                    PendingLeaves = new List<EmployeePendingLeaveDto>()
                };
            }

            // Step 3 : Employee Ids
            var employeeIds = teamMembers
                .Select(x => x.EmployeeId)
                .ToList();

            // Step 4 : Fetch Pending Leave Requests
            var pendingLeaves = await _leaveRepo.Query()
                .Where(x =>
                    employeeIds.Contains(x.EmployeeId) &&
                    x.Status == "Pending")
                .OrderByDescending(x => x.StartDate)
                .ToListAsync(cancellationToken);

            // Step 5 : Response Mapping
            var responseLeaves = pendingLeaves
                .Select(leave =>
                {
                    var employee = teamMembers
                        .FirstOrDefault(x => x.EmployeeId == leave.EmployeeId);

                    return new EmployeePendingLeaveDto
                    {
                        LeaveId = leave.LeaveId,
                        EmployeeName = employee != null
                            ? $"{employee.FirstName} {employee.LastName}"
                            : "Unknown",
                        Type = leave.Type,
                        StartDate = leave.StartDate,
                        EndDate = leave.EndDate,
                        Status = leave.Status,
                        ApprovedByName = null
                    };
                })
                .ToList();

            return new GetEmployeesWithPendingLeavesResponseDto
            {
                PendingLeaves = responseLeaves
            };
        }
    }
}