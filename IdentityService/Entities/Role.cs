using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityService.Entities
{
    [Table("Roles")]
    public class Role 
    {
        [Key]
        [Column("RoleId")]
        public Guid RoleId { get; set; }

        [Required]
        [Column("Name")]
        public required string Name { get; set; }   // ensures non-null

        [Column("Description")]
        public string? Description { get; set; }    // optional
                                                    // 
        public ICollection<User> Users { get; set; } = new List<User>();
        }
    }


