using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeServiceDepartmentService.Entity
{
    [Table("Departments")]
    public class Departments
    {
        [Key]
        [Column("DepartmentId")]
        public Guid DepartmentId { get; set; } = Guid.NewGuid();

        [Required]
        [Column("Name")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [Column("Code")]
        [MaxLength(20)]
        public string Code { get; set; }

        //// Navigation property
        //public ICollection<DepartmentRoles> DepartmentRoles { get; set; }
    }

}
