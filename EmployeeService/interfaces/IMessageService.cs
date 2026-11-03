namespace EmployeeServiceDepartmentService.interfaces
{
    /// <summary>
    /// Abstraction for SMS sending — DIP compliance.
    /// Controllers and handlers depend on this interface, not the concrete Twilio implementation.
    /// </summary>
    public interface IMessageService
    {
        Task SendSmsAsync(string toPhoneNumber, string message);
    }
}
