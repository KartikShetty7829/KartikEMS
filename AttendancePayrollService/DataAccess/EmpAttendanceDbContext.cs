using AttendancePayrollService.Entity;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

namespace AttendancePayrollService.DataAccess
{
    public class EmpAttendanceDbContext : DbContext
    {
        public EmpAttendanceDbContext(DbContextOptions<EmpAttendanceDbContext> options) : base(options) { }

        // Attendance-related tables
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    // AttendanceRecord configuration
        //    modelBuilder.Entity<AttendanceRecord>(entity =>
        //    {
        //        entity.HasKey(e => e.RecordId);

        //        entity.Property(e => e.EmployeeId)
        //              .IsRequired();

        //        entity.Property(e => e.CheckInAt)
        //              .IsRequired();

        //        entity.Property(e => e.Source)
        //              .HasMaxLength(50);

        //        entity.Property(e => e.Notes)
        //              .HasMaxLength(200);

        //        entity.HasOne(e => e.Employee)
        //              .WithMany()
        //              .HasForeignKey(e => e.EmployeeId);
        //    });

        //    base.OnModelCreating(modelBuilder);
        //}
    }

}
