using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendancePayrollService.Entity
{
    [Table("AttendanceRecords")]
    public class AttendanceRecord
    {
        [Key]
        [Column("RecordId")]
        public Guid RecordId { get; set; }

        [Required]
        [Column("EmployeeId")]
        public Guid EmployeeId { get; set; }

        [Required]
        [Column("CheckInAt")]
        public DateTime CheckInAt { get; set; }

        [Column("CheckOutAt")]
        public DateTime? CheckOutAt { get; set; }

        [Column("Source")]
        [MaxLength(50)]
        public string? Source { get; set; }

        [Column("Notes")]
        [MaxLength(200)]
        public string? Notes { get; set; }

        [Column("Status")]
        [MaxLength(20)]
        public string? Status { get; set; }  // SQL: Status NVARCHAR(20) DEFAULT 'Pending'

        [Column("ApprovedBy")]
        public string? ApprovedBy { get; set; }           // SQL: ApprovedBy UNIQUEIDENTIFIER NULL

        [Column("ApprovedAt")]
        public DateTime? ApprovedAt { get; set; }// SQL: ApprovedAt DATETIME2 NULL

        [Column("ApproverId")]
        public Guid? ApproverId { get; set; } // SQL: ApproverId     UNIQUEIDENTIFIER NULL


        // Navigation property to Employees table
        //[ForeignKey(nameof(EmployeeId))]
        //public Employees Employee { get; set; }
    }

}
