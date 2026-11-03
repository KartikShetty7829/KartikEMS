using EmployeeServiceDepartmentService.Entity;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EmployeeServiceDepartmentService.Services
{
    public interface IPayslipService
    {
        Task<string> GeneratePayslipPdfAsync(string employeeName, int employeeNumber, PayrollRecords payroll, Employees employee, string designationTitle, CancellationToken cancellationToken = default);
    }

    public class PayslipService : IPayslipService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<PayslipService> _logger;
        private const string PayslipFolder = "payslips";

        public PayslipService(IWebHostEnvironment environment, ILogger<PayslipService> logger)
        {
            _environment = environment;
            _logger = logger;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<string> GeneratePayslipPdfAsync(string employeeName, int employeeNumber, PayrollRecords payroll, Employees employee, string designationTitle, CancellationToken cancellationToken = default)
        {
            try
            {
                // Create payslip directory if it doesn't exist
                var payslipPath = Path.Combine(_environment.WebRootPath ?? "wwwroot", PayslipFolder);
                if (!Directory.Exists(payslipPath))
                {
                    Directory.CreateDirectory(payslipPath);
                }

                // Generate unique filename
                var fileName = $"Payslip_{employeeNumber}_{payroll.PayrollYear}_{payroll.PayrollMonth:D2}_{Guid.NewGuid().ToString().Substring(0, 8)}.pdf";
                var filePath = Path.Combine(payslipPath, fileName);
                var relativePath = $"/{PayslipFolder}/{fileName}";

                // Generate PDF document
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(20);

                        page.Header().Element(header => ComposeHeader(header, payroll.PayrollMonth, payroll.PayrollYear));
                        page.Content().Element(content => ComposeContent(content, employeeName, employeeNumber, payroll, employee, designationTitle));
                        page.Footer().Element(ComposeFooter);
                    });
                });

                // Save to file
                await Task.Run(() => document.GeneratePdf(filePath), cancellationToken);

                _logger.LogInformation($"Payslip generated successfully for employee {employeeNumber} at {filePath}");
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating payslip for employee {employeeNumber}: {ex.Message}");
                throw;
            }
        }

        private void ComposeHeader(IContainer container, int month, int year)
        {
            var monthName = GetMonthAbbreviation(month);
            var codeCorner = $"{monthName}-{year}";

            container.PaddingTop(15).PaddingBottom(15).Border(1).BorderColor(Colors.Grey.Lighten2).Row(row =>
            {
                row.RelativeColumn(2).Element(col =>
                {
                    col.Column(col =>
                    {
                        col.Item().Text("KARTIK SOFTWARE SOLUTIONS").Bold().FontSize(14).FontColor(Colors.Blue.Darken3);
                        col.Item().Text("Hubli, Karnataka, India | hr@kartiksoftware.com").FontSize(9).FontColor(Colors.Grey.Darken1);
                        col.Item().Text("Startup | Established 2026").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                });

                row.RelativeColumn(1).Element(col =>
                {
                    col.Column(col =>
                    {
                        col.Item().AlignRight().Text(codeCorner).FontSize(9).FontColor(Colors.Grey.Darken1).Italic();
                        col.Item().AlignRight().Text("PAYSLIP").Bold().FontSize(14).FontColor(Colors.Red.Medium);
                    });
                });
            });
        }

        private void ComposeContent(IContainer container, string employeeName, int employeeNumber, PayrollRecords payroll, Employees employee, string designationTitle)
        {
            var monthName = GetMonthName(payroll.PayrollMonth);

            container.PaddingBottom(10).Column(col =>
            {
                // Employee Information Section
                col.Item().Row(row =>
                {
                    row.RelativeColumn(1).Column(c =>
                    {
                        c.Item().Text("Employee Name").FontSize(9).FontColor(Colors.Grey.Darken1);
                        c.Item().PaddingBottom(5).Text(employeeName).Bold().FontSize(11);

                        c.Item().Text("Employee No.").FontSize(9).FontColor(Colors.Grey.Darken1);
                        c.Item().PaddingBottom(5).Text(employeeNumber.ToString()).Bold().FontSize(11);

                        c.Item().Text("Designation").FontSize(9).FontColor(Colors.Grey.Darken1);
                        c.Item().PaddingBottom(5).Text(designationTitle).FontSize(11);

                        c.Item().Text("Department").FontSize(9).FontColor(Colors.Grey.Darken1);
                        c.Item().Text("IT Department").FontSize(11);
                    });

                    row.RelativeColumn(1).Column(c =>
                    {
                        c.Item().Text("Pay Period").FontSize(9).FontColor(Colors.Grey.Darken1);
                        c.Item().PaddingBottom(5).Text(monthName).Bold().FontSize(11);

                        c.Item().Text("Pay Slip ID").FontSize(9).FontColor(Colors.Grey.Darken1);
                        c.Item().PaddingBottom(5).Text(payroll.PayrollRecordId.ToString().Substring(0, 8)).Bold().FontSize(11);

                        c.Item().Text("Bank A/C").FontSize(9).FontColor(Colors.Grey.Darken1);
                        c.Item().PaddingBottom(5).Text("-").FontSize(11);

                        c.Item().Text("PAN").FontSize(9).FontColor(Colors.Grey.Darken1);
                        c.Item().Text("-").FontSize(11);
                    });
                });

                col.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeColumn(1).Column(c =>
                    {
                        c.Item().Text($"Working Days").FontSize(9).FontColor(Colors.Grey.Darken1).Bold();
                        c.Item().PaddingBottom(10).Text((payroll.WorkingDays ?? 0).ToString()).FontSize(20).Bold().FontColor(Colors.Blue.Darken2).AlignCenter();
                    });

                    row.RelativeColumn(1).Column(c =>
                    {
                        c.Item().Text("Present Days").FontSize(9).FontColor(Colors.Grey.Darken1).Bold();
                        c.Item().PaddingBottom(10).Text((payroll.PresentDays ?? 0).ToString()).FontSize(20).Bold().FontColor(Colors.Blue.Darken2).AlignCenter();
                    });

                    row.RelativeColumn(1).Column(c =>
                    {
                        c.Item().Text("Absent / Leave").FontSize(9).FontColor(Colors.Grey.Darken1).Bold();
                        c.Item().PaddingBottom(10).Text((payroll.LeaveDays ?? 0).ToString()).FontSize(20).Bold().FontColor(Colors.Blue.Darken2).AlignCenter();
                    });
                });

                // Earnings Section
                col.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeColumn(1).Column(c =>
                    {
                        c.Item().Background(Colors.Grey.Lighten3).Padding(8).Column(inner =>
                        {
                            inner.Item().Text("EARNINGS").Bold().FontSize(10).FontColor(Colors.Blue.Darken3);
                            inner.Item().PaddingTop(8).Row(r =>
                            {
                                r.RelativeColumn(2).Text("Basic Salary").FontSize(9);
                                r.RelativeColumn(1).Text($"Rs. {payroll.BaseSalary:N2}").Bold().AlignRight().FontSize(9);
                            });
                            inner.Item().PaddingTop(5).Row(r =>
                            {
                                r.RelativeColumn(2).Text("Allowances").FontSize(9);
                                r.RelativeColumn(1).Text($"Rs. {payroll.Allowances:N2}").Bold().AlignRight().FontSize(9);
                            });
                            inner.Item().PaddingTop(5).BorderTop(1).BorderColor(Colors.Grey.Darken1).Row(r =>
                            {
                                r.RelativeColumn(2).Text("Gross Earnings").FontSize(10).Bold();
                                r.RelativeColumn(1).Text($"Rs. {(payroll.BaseSalary + payroll.Allowances):N2}").Bold().AlignRight().FontSize(10);
                            });
                        });
                    });

                    row.RelativeColumn(1).Column(c =>
                    {
                        c.Item().Background(Colors.Grey.Lighten3).Padding(8).Column(inner =>
                        {
                            inner.Item().Text("DEDUCTIONS").Bold().FontSize(10).FontColor(Colors.Blue.Darken3);
                            inner.Item().PaddingTop(8).Row(r =>
                            {
                                r.RelativeColumn(2).Text("Deductions / Adjustments").FontSize(9);
                                r.RelativeColumn(1).Text($"Rs. {payroll.Deductions:N2}").Bold().AlignRight().FontSize(9);
                            });
                            inner.Item().PaddingTop(5).BorderTop(1).BorderColor(Colors.Grey.Darken1).Row(r =>
                            {
                                r.RelativeColumn(2).Text("Total Deductions").FontSize(10).Bold();
                                r.RelativeColumn(1).Text($"Rs. {payroll.Deductions:N2}").Bold().AlignRight().FontSize(10);
                            });
                        });
                    });
                });

                // Net Pay Section
                col.Item().PaddingTop(15).Background(Colors.Teal.Darken1).Padding(12).Row(row =>
                {
                    row.RelativeColumn(2).Column(c =>
                    {
                        c.Item().Text("NET PAY (Take Home)").Bold().FontSize(12).FontColor(Colors.White);
                        c.Item().Text("Forty Five Thousand Rupees and Six Paise Only").FontSize(8).FontColor(Colors.White).Italic();
                    });

                    row.RelativeColumn(1).Column(c =>
                    {
                        c.Item().AlignRight().Text($"Rs. {payroll.NetPay:N2}").Bold().FontSize(16).FontColor(Colors.White);
                        c.Item().AlignRight().Text("Amount in words").FontSize(7).FontColor(Colors.White);
                    });
                });

                col.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeColumn(1).Column(c =>
                    {
                        c.Item().BorderTop(1).BorderColor(Colors.Black).PaddingTop(20).Text("Employee Signature").FontSize(9);
                    });

                    row.RelativeColumn(1).Column(c =>
                    {
                        c.Item().AlignRight().BorderTop(1).BorderColor(Colors.Black).PaddingTop(20).Text("Authorised Signatory").FontSize(9);
                    });
                });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10).Row(row =>
            {
                row.RelativeColumn(1).Text($"Generated on: {DateTime.Now:d MMM yyyy}").FontSize(8).FontColor(Colors.Grey.Darken1);
                row.RelativeColumn(1).AlignRight().Text("Page 1 of 1").FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        }

        private string GetMonthName(int month)
        {
            var date = new DateTime(2024, month, 1);
            return date.ToString("MMMM yyyy");
        }

        private string GetMonthAbbreviation(int month)
        {
            var date = new DateTime(2024, month, 1);
            return date.ToString("MMM").ToUpper();
        }
    }
}
