namespace EmployeeServiceDepartmentService.domains
{
    public class GetAllEmployeeResponseDto
    {
        public List<EmployeeDto> Employees { get; set; }
    }

    // Employee DTO
    public class EmployeeDto
    {
        public Guid EmployeeId { get; set; }
        public int EmployeeNumber { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime HireDate { get; set; }
        public string Status { get; set; }
    }
}
