using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using BakeryPay.Backend.Common;
using BakeryPay.Backend.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BakeryPay.Backend.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpSettings> settings, ILogger<SmtpEmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            return EmailSendResult.NotConfigured("El envio de correos SMTP esta deshabilitado en la configuracion.");
        }

        if (string.IsNullOrWhiteSpace(_settings.Host)
            || string.IsNullOrWhiteSpace(_settings.FromEmail)
            || _settings.Port <= 0)
        {
            return EmailSendResult.NotConfigured("La configuracion SMTP esta incompleta. Revisa Host, Port y FromEmail.");
        }

        try
        {
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = message.Subject,
                Body = string.IsNullOrWhiteSpace(message.HtmlBody) ? message.TextBody : message.HtmlBody,
                IsBodyHtml = !string.IsNullOrWhiteSpace(message.HtmlBody)
            };

            mailMessage.To.Add(new MailAddress(message.ToEmail, message.ToName));

            if (!string.IsNullOrWhiteSpace(message.TextBody) && !string.IsNullOrWhiteSpace(message.HtmlBody))
            {
                mailMessage.AlternateViews.Add(
                    AlternateView.CreateAlternateViewFromString(message.TextBody, null, "text/plain"));
            }

            foreach (var attachment in message.Attachments.Where(x => !string.IsNullOrWhiteSpace(x.FilePath) && File.Exists(x.FilePath)))
            {
                var mailAttachment = new Attachment(attachment.FilePath, attachment.ContentType);
                mailAttachment.Name = string.IsNullOrWhiteSpace(attachment.FileName)
                    ? Path.GetFileName(attachment.FilePath)
                    : attachment.FileName;
                mailAttachment.ContentDisposition!.Inline = false;
                mailAttachment.ContentDisposition.DispositionType = DispositionTypeNames.Attachment;
                mailMessage.Attachments.Add(mailAttachment);
            }

            using var smtpClient = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.UseSsl,
                UseDefaultCredentials = _settings.UseDefaultCredentials
            };

            if (!_settings.UseDefaultCredentials && !string.IsNullOrWhiteSpace(_settings.Username))
            {
                smtpClient.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
            }

            cancellationToken.ThrowIfCancellationRequested();
            await smtpClient.SendMailAsync(mailMessage);
            return EmailSendResult.Sent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No fue posible enviar el correo a {ToEmail}.", message.ToEmail);
            return EmailSendResult.Failed($"No fue posible enviar el correo: {ex.Message}");
        }
    }
}
