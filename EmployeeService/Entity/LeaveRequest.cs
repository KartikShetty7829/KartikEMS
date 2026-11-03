using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeServiceDepartmentService.Entity
{
    [Table("LeaveRequests")]
    public class LeaveRequests
    {
        [Key]
        public Guid LeaveId { get; set; }
        public Guid EmployeeId { get; set; }
        public required string Type { get; set; } 
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public required string Status { get; set; } 
        public Guid? ApprovedBy { get; set; }

        [Column("ApprovedId")]
        public Guid? ApproverdId { get; set; }
    }
}
