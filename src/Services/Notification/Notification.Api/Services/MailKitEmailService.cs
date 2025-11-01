using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Notification.Api.Models;
using System;
using System.Threading.Tasks;

namespace Notification.Api.Services
{
    public class MailKitEmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly IEmailTemplateService _templateService;
        private readonly ILogger<MailKitEmailService> _logger;

        public MailKitEmailService(
            IOptions<SmtpSettings> smtpSettings,
            IEmailTemplateService templateService,
            ILogger<MailKitEmailService> logger)
        {
            _smtpSettings = smtpSettings.Value;
            _templateService = templateService;
            _logger = logger;
        }

        public async Task SendEmailAsync(EmailMessage message)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromEmail));
            mimeMessage.To.Add(MailboxAddress.Parse(message.To));
            mimeMessage.Subject = message.Subject;

            var builder = new BodyBuilder();

            if (!string.IsNullOrEmpty(message.HtmlBody))
            {
                builder.HtmlBody = message.HtmlBody;
            }

            if (!string.IsNullOrEmpty(message.TextBody))
            {
                builder.TextBody = message.TextBody;
            }

            // Agregar adjuntos si existen
            foreach (var attachment in message.Attachments)
            {
                builder.Attachments.Add(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));
            }

            mimeMessage.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                // Conectar al servidor SMTP
                await client.ConnectAsync(
                    _smtpSettings.Host,
                    _smtpSettings.Port,
                    _smtpSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

                // Autenticar
                await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);

                // Enviar email
                await client.SendAsync(mimeMessage);

                // Desconectar
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email sent successfully to {message.To} - Subject: {message.Subject}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {message.To}");
                throw;
            }
        }

        public async Task SendTemplatedEmailAsync(string to, string templateName, object data)
        {
            try
            {
                // Renderizar template
                var (subject, htmlBody, textBody) = await _templateService.RenderTemplateAsync(templateName, data);

                // Crear mensaje
                var message = new EmailMessage
                {
                    To = to,
                    Subject = subject,
                    HtmlBody = htmlBody,
                    TextBody = textBody
                };

                // Enviar
                await SendEmailAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending templated email to {to} using template {templateName}");
                throw;
            }
        }
    }
}
