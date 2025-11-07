using NetworkMonitoringSystem.Core.Data;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Сервис для отправки email уведомлений
    /// </summary>
    public class EmailService
    {
        private readonly ApplicationDbContext _context;

        public EmailService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Отправить email уведомление
        /// </summary>
        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            try
            {
                var settings = GetEmailSettings();
                if (string.IsNullOrEmpty(settings.SmtpServer) || string.IsNullOrEmpty(settings.FromEmail))
                {
                    System.Diagnostics.Debug.WriteLine("Email настройки не сконфигурированы");
                    return false;
                }

                using var message = new MailMessage(settings.FromEmail!, to, subject, body)
                {
                    IsBodyHtml = isHtml,
                    BodyEncoding = Encoding.UTF8,
                    SubjectEncoding = Encoding.UTF8
                };

                using var client = new SmtpClient(settings.SmtpServer, settings.SmtpPort)
                {
                    EnableSsl = settings.UseSsl,
                    Credentials = string.IsNullOrEmpty(settings.Username) 
                        ? null 
                        : new NetworkCredential(settings.Username, settings.Password)
                };

                await client.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка отправки email: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Отправить уведомление об изменении статуса устройства
        /// </summary>
        public async Task SendDeviceStatusAlertAsync(NetworkDevice device, DeviceStatus oldStatus, DeviceStatus newStatus)
        {
            var users = await _context.Users
                .Where(u => u.IsActive && !string.IsNullOrEmpty(u.Email))
                .ToListAsync();

            var subject = $"Изменение статуса устройства: {device.Name}";
            var body = new StringBuilder();
            body.AppendLine($"Устройство: {device.Name}");
            body.AppendLine($"IP адрес: {device.IpAddress}");
            body.AppendLine($"Предыдущий статус: {oldStatus}");
            body.AppendLine($"Новый статус: {newStatus}");
            body.AppendLine($"Время: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");

            foreach (var user in users)
            {
                await SendEmailAsync(user.Email!, subject, body.ToString());
            }
        }

        /// <summary>
        /// Отправить уведомление о новой заявке на обслуживание
        /// </summary>
        public async Task SendMaintenanceRequestNotificationAsync(MaintenanceRequest request)
        {
            var adminUsers = await _context.Users
                .Where(u => u.IsActive && u.Role == UserRole.Admin && !string.IsNullOrEmpty(u.Email))
                .ToListAsync();

            var device = await _context.NetworkDevices.FindAsync(request.DeviceId);
            var user = await _context.Users.FindAsync(request.UserId);

            var subject = $"Новая заявка на обслуживание: {request.Title}";
            var body = new StringBuilder();
            body.AppendLine($"Заявка: {request.Title}");
            body.AppendLine($"Устройство: {device?.Name ?? "Неизвестно"}");
            body.AppendLine($"Создал: {user?.FullName ?? "Неизвестно"}");
            body.AppendLine($"Приоритет: {request.Priority}");
            body.AppendLine($"Описание: {request.Description ?? "Нет описания"}");

            foreach (var admin in adminUsers)
            {
                await SendEmailAsync(admin.Email!, subject, body.ToString());
            }
        }

        /// <summary>
        /// Отправить уведомление о критическом статусе устройства
        /// </summary>
        public async Task SendCriticalAlertAsync(NetworkDevice device)
        {
            var adminUsers = await _context.Users
                .Where(u => u.IsActive && u.Role == UserRole.Admin && !string.IsNullOrEmpty(u.Email))
                .ToListAsync();

            var subject = $"КРИТИЧЕСКОЕ: Устройство {device.Name} недоступно";
            var body = new StringBuilder();
            body.AppendLine("КРИТИЧЕСКОЕ УВЕДОМЛЕНИЕ");
            body.AppendLine($"Устройство {device.Name} (IP: {device.IpAddress}) недоступно!");
            body.AppendLine($"Последний контакт: {device.LastSeen?.ToString("dd.MM.yyyy HH:mm:ss") ?? "Никогда"}");
            body.AppendLine($"Требуется немедленное внимание!");

            foreach (var admin in adminUsers)
            {
                await SendEmailAsync(admin.Email!, subject, body.ToString());
            }
        }

        /// <summary>
        /// Получить настройки email из конфигурации
        /// </summary>
        private EmailSettings GetEmailSettings()
        {
            var settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NetworkMonitoringSystem",
                "email_settings.json");

            if (File.Exists(settingsPath))
            {
                try
                {
                    var json = File.ReadAllText(settingsPath);
                    return System.Text.Json.JsonSerializer.Deserialize<EmailSettings>(json) ?? new EmailSettings();
                }
                catch { }
            }

            return new EmailSettings();
        }

        private class EmailSettings
        {
            public string? SmtpServer { get; set; }
            public int SmtpPort { get; set; } = 587;
            public string? FromEmail { get; set; }
            public string? Username { get; set; }
            public string? Password { get; set; }
            public bool UseSsl { get; set; } = true;
        }
    }
}
