namespace EmployeeServiceDepartmentService.interfaces
{
    /// <summary>
    /// Abstraction for email sending — DIP compliance.
    /// Controllers and handlers depend on this interface, not the concrete SmtpClient implementation.
    /// </summary>
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
