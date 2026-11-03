using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeServiceDepartmentService.Entity
{
    [Table("EmployeeAddresses")]
    public class EmployeeAddress
    {
        [Key]
        public Guid AddressId { get; set; }   // PK handled by SQL Server (NEWID or IDENTITY)

        [Required]
        public Guid EmployeeId { get; set; }  // FK to Employees table

        [Required, MaxLength(200)]
        public required string AddressLine1 { get; set; } 

        [MaxLength(200)]
        public string? AddressLine2 { get; set; }

        [Required, MaxLength(100)]
        public required string City { get; set; } 

        [Required, MaxLength(100)]
        public required string State { get; set; } 

        [Required, MaxLength(20)]
        public required string PostalCode { get; set; } 

        [Required, MaxLength(100)]
        public required string Country { get; set; } 

        [Required, MaxLength(50)]
        public required string AddressType { get; set; }  // e.g. Permanent, Current, Mailing

        //// Navigation property (optional, for EF relationships)
        //[ForeignKey(nameof(EmployeeId))]
        //public Employees Employee { get; set; } = null!;
    }
}

