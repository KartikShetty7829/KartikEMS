using MediatR;

namespace EmployeeServiceDepartmentService.domains
{
    public class EmployeeRecordUpdateRequestDto : IRequest<EmployeeRecordUpdateResponseDto>
    {
        public int EmployeeNumber { get; set; }

        // Optional fields to update
        public string FirstName { get; set; }
        public string LastName { get; set; }       
        public string? phonenumber { get; set; }
        public string  Email { get; set; }
        public DateTime? HireDate { get; set; }

       
    }
}
