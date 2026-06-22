using BakeryPay.Backend.Common;

namespace BakeryPay.Backend.Interfaces.Services;

public interface IEmailSender
{
    Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
