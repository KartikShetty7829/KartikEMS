using System;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Entities
{
    public class Projects
    {
        [Key]
        public Guid ProjectId { get; set; } 
        // UNIQUEIDENTIFIER
        public required string Name { get; set; }      // NVARCHAR(150)
        public string? Description { get; set; } // NVARCHAR(500)
        public DateTime StartDate { get; set; }  // DATE
        public DateTime? EndDate { get; set; }   // DATE
        public required string Status { get; set; }    // NVARCHAR(50)
    }
}

