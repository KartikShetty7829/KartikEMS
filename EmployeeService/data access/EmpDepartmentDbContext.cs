using EmployeeManagementSystem.Entities;
using EmployeeServiceDepartmentService.Entity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EmployeeServiceDepartmentService.data_access
{
    public class EmpDepartmentDbContext : DbContext
    {
        public EmpDepartmentDbContext(DbContextOptions<EmpDepartmentDbContext> options) : base(options) { }

        // Employee-related tables
        public DbSet<Employees> Employees { get; set; }

       

        public DbSet<JobContracts> JobContracts { get; set; }
        public DbSet<Attachments> Attachments { get; set; }

        // Department-related tables
        public DbSet<Departments> Departments { get; set; }
        public DbSet<DepartmentRoles> DepartmentRoles { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<EmployeeDesignation>EmployeeDesignations { get; set; }
        
        public DbSet<EmployeeAddress> employeeAddresses { get; set; }

        public DbSet<Projects> Projects { get; set; }

        public DbSet<EmployeeProject>EmployeeProjects { get; set; }

        public DbSet<LeaveRequests> leaveRequest { get; set; }

        public DbSet<PayrollRecords> PayrollRecords { get; set; }

        public DbSet<Payslip> Payslips { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure EmployeeNumber as identity and ignore updates
            modelBuilder.Entity<Employees>()
                .Property(e => e.EmployeeNumber)
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
        }
    }
}
