using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeServiceDepartmentService.Entity
{
    [Table("Payslips")] // SQL table name
    public class Payslip
    {
        [Key]
        [Column("PayslipId")]
        public Guid PayslipId { get; set; }   // UNIQUEIDENTIFIER PRIMARY KEY

        [Required]
        [ForeignKey("Employee")] // SQL reference to Employees(EmployeeId)
        [Column("EmployeeId")]
        public Guid EmployeeId { get; set; }  // UNIQUEIDENTIFIER NOT NULL

        [Column("PdfUrl", TypeName = "nvarchar(500)")]
        public string PdfUrl { get; set; }    // NVARCHAR(500) for storing PDF path or URL

        //[Required]
        //[Column("GeneratedDate")]
        //public DateTime GeneratedDate { get; set; } = DateTime.Now; // Default to current date

        // Navigation property for Employee reference
        public virtual Employees Employee { get; set; }
    }
}

