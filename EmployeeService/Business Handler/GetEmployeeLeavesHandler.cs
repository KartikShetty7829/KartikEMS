using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.domains;
using EmployeeServiceDepartmentService.Entity;
using EmployeeServiceDepartmentService.Exceptions;
using EmployeeServiceDepartmentService.interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeServiceDepartmentService.Business_Handler
{
    public class GetEmployeeLeavesHandler
        : IRequestHandler<GetEmployeeLeavesRequestDto, GetEmployeeLeavesResponseDto>
    {
        private readonly IGenericRepository<EmpDepartmentDbContext, LeaveRequests> _leaveRepo;
        private readonly IGenericRepository<EmpDepartmentDbContext, Employees> _employeeRepo;
        private readonly ILogger<GetEmployeeLeavesHandler> _logger;

        public GetEmployeeLeavesHandler(
            IGenericRepository<EmpDepartmentDbContext, LeaveRequests> leaveRepo,
            IGenericRepository<EmpDepartmentDbContext, Employees> employeeRepo,
            ILogger<GetEmployeeLeavesHandler> logger)
        {
            _leaveRepo = leaveRepo;
            _employeeRepo = employeeRepo;
            _logger = logger;
        }

        public async Task<GetEmployeeLeavesResponseDto> Handle(
            GetEmployeeLeavesRequestDto request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching leave requests for EmployeeNumber {EmployeeNumber}", request.EmployeeNumber);

                // Resolve employee from EmployeeNumber
                var employee = await _employeeRepo.Query()
                    .FirstOrDefaultAsync(e => e.EmployeeNumber == request.EmployeeNumber, cancellationToken);

                if (employee == null)
                {
                    throw new NotFoundException("Employee not found.");
                }

                // Fetch all leave requests for this employee
                var leaves = await _leaveRepo.Query()
                    .Where(l => l.EmployeeId == employee.EmployeeId)
                    .ToListAsync(cancellationToken);

                var responseLeaves = leaves.Select(l =>
                {
                    return new EmployeeLeaveDto
                    {
                        LeaveId = l.LeaveId,
                        Type = l.Type,
                        StartDate = l.StartDate,
                        EndDate = l.EndDate,
                        Status = l.Status,
                    };

                }).ToList();

                return new GetEmployeeLeavesResponseDto
                {
                    Leaves = responseLeaves,
                    Message = "Leave requests fetched successfully."
                };
        }
    }
}
