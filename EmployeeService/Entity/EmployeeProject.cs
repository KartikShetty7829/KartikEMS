using EmployeeManagementSystem.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeServiceDepartmentService.Entity
{
    public class EmployeeProject
    {
        [Key]
        public Guid EmployeeProjectId { get; set; }   // Primary Key
       
        public Guid EmployeeId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid DesignationId { get; set; }             // e.g., Developer, Manager
        public DateTime AssignedDate { get; set; }
        public DateTime? ReleaseDate { get; set; }

        // ✅ Navigation properties
        [ForeignKey(nameof(EmployeeId))]
        public Employees Employee { get; set; }


        [ForeignKey(nameof(ProjectId))]
        public Projects Project { get; set; }

    }

}
