using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeServiceDepartmentService.Entity
{
    [Table("DepartmentRoles")]
    public class DepartmentRoles
    {
        [Key]
        [Column("DepartmentRoleId")]
        public Guid DepartmentRoleId { get; set; } = Guid.NewGuid();

        [Required]
        [Column("DepartmentId")]
        public Guid DepartmentId { get; set; }

        [Required]
        [Column("Name")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Column("Description")]
        [MaxLength(200)]
        public string Description { get; set; }

        //// Navigation property
        //public Departments Department { get; set; }
    }

}
