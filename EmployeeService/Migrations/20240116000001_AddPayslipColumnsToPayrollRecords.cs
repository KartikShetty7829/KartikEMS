using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeServiceDepartmentService.Migrations
{
    /// <inheritdoc />
    public partial class AddPayslipColumnsToPayrollRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PayslipPdfUrl",
                table: "PayrollRecords",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PayslipGeneratedDate",
                table: "PayrollRecords",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayslipPdfUrl",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "PayslipGeneratedDate",
                table: "PayrollRecords");
        }
    }
}
