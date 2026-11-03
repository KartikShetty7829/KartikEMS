using System.ComponentModel.DataAnnotations;

namespace EmployeeServiceDepartmentService.domains
{
    public class EmployeeProfileResponseDto
    {
        public Guid EmployeeId { get; set; }
        public int EmployeeNumber { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime HireDate { get; set; }
        public bool Status { get; set; }

     // public string? ProfileImageUrl { get; set; }

      
        public required string AddressLine1 { get; set; }

        public string? AddressLine2 { get; set; }
        public required string City { get; set; }

      
        public required string State { get; set; }

      
        public required string PostalCode { get; set; }


        public required string Country { get; set; }

        
        public required string AddressType { get; set; }
    }

}
