using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityService.Entities
{
    [Table("Users")]
    public class User 
    {
        [Key]
        [Column("UserId")]
        public Guid UserId { get; set; } = Guid.NewGuid();   // GUID primary key

        [Required]
        [Column("Username")]
        public required string Username { get; set; }

        [Required]
        [Column("Password")]
        public required string Password { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        
        [Column("RoleId")]
        public Guid RoleId { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("Employeenumber")]
        public int EmployeeNumber { get; set; }  // optional FK     

        public  Role Role { get; set; }

    }
}
