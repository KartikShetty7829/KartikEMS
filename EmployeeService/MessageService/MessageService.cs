using EmployeeServiceDepartmentService.interfaces;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace EmployeeServiceDepartmentService.MessageServices
{
    /// <summary>
    /// Concrete SMS service using Twilio — implements IMessageService (DIP).
    /// </summary>
    public class MessageService : IMessageService
    {
        private readonly TwilioSettings _twilioSettings;

        public MessageService(IOptions<TwilioSettings> twilioSettings)
        {
            _twilioSettings = twilioSettings.Value;
            TwilioClient.Init(_twilioSettings.AccountSid, _twilioSettings.AuthToken);
        }

        public async Task SendSmsAsync(string toPhoneNumber, string message)
        {
            await MessageResource.CreateAsync(
                body: message,
                from: new Twilio.Types.PhoneNumber(_twilioSettings.FromPhoneNumber),
                to: new Twilio.Types.PhoneNumber(toPhoneNumber)
            );
        }
    }
}
