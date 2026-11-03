using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeServiceDepartmentService.Entity
{
    [Table("PayrollRecords")] // SQL table name
    public class PayrollRecords
    {
        [Key]
        [Column("PayrollRecordId")]
        public Guid PayrollRecordId { get; set; }  // UNIQUEIDENTIFIER PRIMARY KEY

        [Required]
        [ForeignKey("Employee")] // SQL reference to Employees(EmployeeId)
        [Column("EmployeeId")]
        public Guid EmployeeId { get; set; }       // UNIQUEIDENTIFIER NOT NULL

        [Required]
        [Column("PayrollMonth")]
        public int PayrollMonth { get; set; }      // INT NOT NULL (1 = Jan, 12 = Dec)

        [Required]
        [Column("PayrollYear")]
        public int PayrollYear { get; set; }       // INT NOT NULL (e.g., 2025)

        [Required]
        [Column("BaseSalary", TypeName = "decimal(18,2)")]
        public decimal BaseSalary { get; set; }    // DECIMAL(18,2) NOT NULL

        [Column("Allowances", TypeName = "decimal(18,2)")]
        public decimal Allowances { get; set; }    // DECIMAL(18,2) DEFAULT 0

        [Column("Deductions", TypeName = "decimal(18,2)")]
        public decimal Deductions { get; set; }    // DECIMAL(18,2) DEFAULT 0

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("NetPay", TypeName = "decimal(18,2)")]
        public decimal NetPay { get; private set; } // Computed column (BaseSalary + Allowances - Deductions)

        [Required]
        [Column("GeneratedDate")]
        public DateTime GeneratedDate { get; set; } // DATETIME NOT NULL DEFAULT GETDATE()

        [Required]
        [Column("WorkingDays")]
        public int? WorkingDays { get; set; }

        [Required]
        [Column("PresentDays")]
        public int? PresentDays { get; set; }


        [Required]
        [Column("LeaveDays")]
        public int? LeaveDays { get; set; }


        [Required]
        [Column("HolidayDays")]
        public int? HolidayDays { get; set; }
        [Required]
        [Column("WeekendDays")]
        public int? WeekendDays { get; set; }
        [Required]
        [Column("PayableDays")] 
        public int? PayableDays { get; set; }

        [Column("PayslipPdfUrl", TypeName = "nvarchar(500)")]
        public string? PayslipPdfUrl { get; set; }  // NVARCHAR(500) for PDF path/URL

        [Column("PayslipGeneratedDate")]
        public DateTime? PayslipGeneratedDate { get; set; }  // DATETIME when payslip was generated

        // Navigation property for Employee reference
        public virtual Employees Employee { get; set; }
    }
}
