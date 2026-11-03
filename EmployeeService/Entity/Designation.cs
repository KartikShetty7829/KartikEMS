using System;

namespace EmployeeServiceDepartmentService.Entity
{
    public class Designation
    {
        public Guid DesignationId { get; set; }   // UNIQUEIDENTIFIER PK
        public required string Title { get; set; }         // NVARCHAR(100)
        public string? Description { get; set; }   // NVARCHAR(250)


    }
}
