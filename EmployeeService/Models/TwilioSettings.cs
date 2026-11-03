namespace EmployeeServiceDepartmentService.MessageServices
{
    /// <summary>
    /// Twilio configuration settings — separated from MessageService for SRP.
    /// </summary>
    public class TwilioSettings
    {
        public required string AccountSid { get; set; }
        public required string AuthToken { get; set; }
        public required string FromPhoneNumber { get; set; }
    }
}
