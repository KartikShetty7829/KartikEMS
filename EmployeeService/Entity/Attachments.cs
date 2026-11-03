using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeServiceDepartmentService.Entity
{
    [Table("Attachments")]
    public class Attachments
    {
        [Key]
        [Column("AttachmentId")]
        public Guid AttachmentId { get; set; } = Guid.NewGuid();

        [Required]
        [Column("EmployeeId")]
        public Guid EmployeeId { get; set; }

        [Required]
        [Column("FileName")]
        [MaxLength(200)]
        public string FileName { get; set; }

        [Required]
        [Column("Url")]
        [MaxLength(500)]
        public string Url { get; set; }

        [Column("Type")]
        [MaxLength(50)]
        public string Type { get; set; }

        [Column("UploadedAt")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
      //  public Employees Employee { get; set; }
    }

}
