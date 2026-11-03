using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeServiceDepartmentService.Entity
{
    [Table("JobContracts")]
    public class JobContracts
    {
        [Key]
        [Column("ContractId")]
        public Guid ContractId { get; set; } = Guid.NewGuid();

        [Required]
        [Column("EmployeeId")]
        public Guid EmployeeId { get; set; }

        [Required]
        [Column("Title")]
        [MaxLength(100)]
        public string Title { get; set; }

        [Column("Grade")]
        [MaxLength(50)]
        public string Grade { get; set; }

        [Required]
        [Column("BaseSalary")]
        public decimal BaseSalary { get; set; }

        [Required]
        [Column("StartDate")]
        public DateTime StartDate { get; set; }

        [Column("EndDate")]
        public DateTime? EndDate { get; set; }

        //// Navigation property
        //public Employees Employee { get; set; }
    }
}
