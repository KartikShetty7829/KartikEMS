using EmployeeServiceDepartmentService.Common;
using MediatR;

namespace EmployeeServiceDepartmentService.domains
{
    public class EmployeeRecordDeleteRequestDto : IRequest<EmployeeRecordDeleteResponseDto>
    {
        public int EmployeeNumber { get; set; }      
        public EmployeeStatus Status { get; set; } // use enum instead of int
    }
}

