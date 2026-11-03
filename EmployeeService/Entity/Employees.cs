using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mail;

namespace EmployeeServiceDepartmentService.Entity
{
    [Table("Employees")]
    public class Employees
    {
        [Key]
        [Column("EmployeeId")]
        public Guid EmployeeId { get; set; } = Guid.NewGuid();

        [Required]
        [Column("FirstName")]
        [MaxLength(100)]
        public required string FirstName { get; set; }

        [Required]
        [Column("LastName")]
        [MaxLength(100)]
        public required string LastName { get; set; }

        [Required]
        [Column("Email")]
        [MaxLength(150)]
        public required string Email { get; set; }

        [Column("Phone")]
        [MaxLength(20)]
        public required string Phone { get; set; }

        [Required]
        [Column("HireDate")]
        public DateTime HireDate { get; set; }

        [Required]
        [Column("Status")]
        public bool Status { get; set; }

        [Column("ProfileImageUrl")]
        public string? ProfileImageUrl { get; set; }


        [Column("EmployeeNumber")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]   // 👈 marks it as identity
        public int EmployeeNumber { get; set; }

        [Column("ManagerId")]
        public Guid? ManagerId { get; set; }

        [Column("MonthlySalary")]
        public decimal? MonthlySalary { get; set; }

        // Navigation properties
        //public ICollection<JobContracts> JobContracts { get; set; }
        //public ICollection<Attachments> Attachments { get; set; }
    }

}
