using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace SoloBill.Services
{
    public class EmailSettings
    {
        public string? FromName { get; set; }
        public string FromEmail { get; set; } = "";

      
        public string Mode { get; set; } = "Smtp";

        public bool UseSendGrid { get; set; } = true; 
        public string? SendGridApiKey { get; set; }

        public string SmtpHost { get; set; } = "localhost";
        public int SmtpPort { get; set; } = 25;
        public string? SmtpUser { get; set; }
        public string? SmtpPass { get; set; }
    }

    public interface IEmailService
    {
        Task SendInvoiceAsync(
            string toEmail,
            string subject,
            string htmlBody,
            byte[] pdfBytes,
            string pdfFileName);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _cfg;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> cfg, ILogger<EmailService> logger)
        {
            _cfg = cfg.Value;
            _logger = logger;
        }

        public async Task SendInvoiceAsync(
            string toEmail,
            string subject,
            string htmlBody,
            byte[] pdfBytes,
            string pdfFileName)
        {
            var mode = (_cfg.Mode ?? "").Trim().ToLowerInvariant();

            if (mode == "logonly")
            {
                _logger.LogInformation(
                    "LOG-ONLY email to {To}. Subject: {Subject}. Attachment: {FileName} ({Bytes} bytes)",
                    toEmail, subject, pdfFileName, pdfBytes?.Length ?? 0);
                return;
            }

            if (mode == "sendgrid" || (_cfg.UseSendGrid && !string.IsNullOrWhiteSpace(_cfg.SendGridApiKey)))
            {
                await SendWithSendGridAsync(toEmail, subject, htmlBody, pdfBytes, pdfFileName);
                return;
            }

            // default to SMTP
            await SendWithSmtpAsync(toEmail, subject, htmlBody, pdfBytes, pdfFileName);
        }

        private async Task SendWithSendGridAsync(
            string toEmail, string subject, string htmlBody, byte[] pdfBytes, string pdfFileName)
        {
            if (string.IsNullOrWhiteSpace(_cfg.SendGridApiKey))
                throw new InvalidOperationException("SendGrid mode selected but SendGridApiKey is missing.");

            var client = new SendGridClient(_cfg.SendGridApiKey);
            var from = new EmailAddress(_cfg.FromEmail, _cfg.FromName ?? _cfg.FromEmail);
            var to = new EmailAddress(toEmail);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent: htmlBody);
            if (pdfBytes is { Length: > 0 })
            {
                msg.AddAttachment(pdfFileName, Convert.ToBase64String(pdfBytes), "application/pdf");
            }

            var resp = await client.SendEmailAsync(msg);
            if ((int)resp.StatusCode >= 400)
            {
                var body = await resp.Body.ReadAsStringAsync();
                throw new InvalidOperationException($"SendGrid failed: {(int)resp.StatusCode} - {body}");
            }
        }

        private async Task SendWithSmtpAsync(
            string toEmail, string subject, string htmlBody, byte[] pdfBytes, string pdfFileName)
        {
            using var msg = new MailMessage
            {
                From = new MailAddress(_cfg.FromEmail, _cfg.FromName ?? _cfg.FromEmail),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            msg.To.Add(toEmail);

            if (pdfBytes is { Length: > 0 })
            {
                
                msg.Attachments.Add(new System.Net.Mail.Attachment(
                    new MemoryStream(pdfBytes), pdfFileName, "application/pdf"));
            }

            using var client = new SmtpClient(_cfg.SmtpHost, _cfg.SmtpPort)
            {
                EnableSsl = true
            };

            if (!string.IsNullOrWhiteSpace(_cfg.SmtpUser))
            {
                client.Credentials = new NetworkCredential(_cfg.SmtpUser, _cfg.SmtpPass);
            }

            await client.SendMailAsync(msg);
        }
    }
}