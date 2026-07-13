using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using CrmArcheonzero.Models;

namespace CrmArcheonzero.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;
        private readonly bool _isEnabled;

        public EmailService(EmailSettings settings)
        {
            _settings = settings;
            _isEnabled = settings != null &&
                         !string.IsNullOrEmpty(settings.SenderEmail) &&
                         settings.SenderEmail != "your-email@gmail.com";
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            if (!_isEnabled) return false;

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("CRM System", _settings.SenderEmail));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                if (isHtml)
                    bodyBuilder.HtmlBody = body;
                else
                    bodyBuilder.TextBody = body;

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                client.Timeout = 30000;

                await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort,
                    _settings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_settings.SenderEmail, _settings.SenderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "EmailService.SendEmailAsync");
                return false;
            }
        }

        public async Task SendWelcomeEmail(string to, string name)
        {
            if (!_isEnabled) return;
            var subject = "Добро пожаловать в CRM систему!";
            var body = $@"
                <h1>Здравствуйте, {name}!</h1>
                <p>Рады приветствовать вас в нашей CRM системе.</p>
                <p>С уважением,<br/>Команда CRM</p>";
            await SendEmailAsync(to, subject, body, true);
        }

        public async Task SendBirthdayNotification(string to, string clientName)
        {
            if (!_isEnabled) return;
            var subject = $"🎂 День рождения клиента!";
            var body = $@"
                <h1>Сегодня день рождения!</h1>
                <p>Клиент <b>{clientName}</b> отмечает день рождения.</p>
                <p>Не забудьте поздравить!</p>";
            await SendEmailAsync(to, subject, body, true);
        }

        public async Task SendTaskReminder(string to, string taskTitle, DateTime dueDate)
        {
            if (!_isEnabled) return;
            var subject = $"🔔 Напоминание о задаче: {taskTitle}";
            var body = $@"
                <h2>Напоминание о задаче</h2>
                <p><b>Задача:</b> {taskTitle}</p>
                <p><b>Срок выполнения:</b> {dueDate:dd.MM.yyyy HH:mm}</p>
                <p>Пожалуйста, выполните задачу вовремя.</p>";
            await SendEmailAsync(to, subject, body, true);
        }
    }
}