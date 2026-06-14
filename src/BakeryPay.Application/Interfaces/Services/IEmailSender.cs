using BakeryPay.Application.Common;

namespace BakeryPay.Application.Interfaces.Services;

public interface IEmailSender
{
    Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
