using System;
using System.ComponentModel.DataAnnotations;

namespace EmployeeServiceDepartmentService.Entity
{
    public class Notification
    {
        [Key]
        public Guid NotificationId { get; set; } = Guid.NewGuid();
        
        public Guid EmployeeId { get; set; } // The person who should see this notification
        
        public string Message { get; set; }
        
        public bool IsRead { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Optional: Reference to the leave request to make deletion easier
        public Guid? LeaveRequestId { get; set; }
    }
}
