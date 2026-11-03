using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeServiceDepartmentService.Entity
{
    [Table("EmployeeDesignations")]
    public class EmployeeDesignation
    {
        [Key]
        public Guid EmployeeDesignationId { get; set; }   // PK handled by SQL Server (NEWID or IDENTITY)

        [Required]
        public Guid EmployeeId { get; set; }              // FK to Employees table

        [Required]
        public Guid DesignationId { get; set; }           // FK to Designations table

        [Required]
        public DateTime StartDate { get; set; }           // When designation starts

        public DateTime? EndDate { get; set; }            // Optional: when designation ends

        // ✅ Navigation properties
        [ForeignKey(nameof(EmployeeId))]
        public Employees Employee { get; set; }

        [ForeignKey(nameof(DesignationId))]
        public Designation Designation { get; set; }
    }
}
