using MediatR;
using System.ComponentModel.DataAnnotations;

namespace EmployeeServiceDepartmentService.domains
{
    public class UpdateEmployeeProfileRequestDto : IRequest<UpdateEmployeeProfileResponseDto>
    {
        [Required]
        public int EmployeeNumber { get; set; }

        [MaxLength(200)]
        public string AddressLine1 { get; set; }

        [MaxLength(200)]
        public string? AddressLine2 { get; set; }

        [MaxLength(100)]
        public string City { get; set; }

        [MaxLength(100)]
        public string State { get; set; }

        [MaxLength(20)]
        public string PostalCode { get; set; }

        [MaxLength(100)]
        public string Country { get; set; }

        [MaxLength(50)]
        public string AddressType { get; set; }

        // Optional: allow updating other employee fields
        public string? Email { get; set; }
        public string? Phone { get; set; }
      
    }
}
