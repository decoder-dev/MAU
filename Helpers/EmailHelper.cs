using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Core.Data;

namespace NetworkMonitoringSystem.Helpers
{
    /// <summary>
    /// Вспомогательный класс для работы с email
    /// </summary>
    public static class EmailHelper
    {
        /// <summary>
        /// Отправить email уведомление
        /// </summary>
        public static async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var context = Program.DbContext;
                var emailService = new EmailService(context);
                return await emailService.SendEmailAsync(to, subject, body);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Отправить уведомление об изменении статуса устройства
        /// </summary>
        public static async Task SendDeviceStatusNotificationAsync(
            NetworkMonitoringSystem.Core.Models.NetworkDevice device,
            NetworkMonitoringSystem.Core.Models.DeviceStatus oldStatus,
            NetworkMonitoringSystem.Core.Models.DeviceStatus newStatus)
        {
            try
            {
                using var context = Program.DbContext;
                var emailService = new EmailService(context);
                await emailService.SendDeviceStatusAlertAsync(device, oldStatus, newStatus);
            }
            catch { }
        }

        /// <summary>
        /// Отправить критическое уведомление
        /// </summary>
        public static async Task SendCriticalNotificationAsync(
            NetworkMonitoringSystem.Core.Models.NetworkDevice device)
        {
            try
            {
                using var context = Program.DbContext;
                var emailService = new EmailService(context);
                await emailService.SendCriticalAlertAsync(device);
            }
            catch { }
        }

        /// <summary>
        /// Проверить настройки email
        /// </summary>
        public static bool ValidateEmailSettings()
        {
            try
            {
                var settingsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "NetworkMonitoringSystem",
                    "email_settings.json");

                if (!File.Exists(settingsPath))
                    return false;

                var json = File.ReadAllText(settingsPath);
                var settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                return settings != null &&
                       settings.ContainsKey("SmtpServer") &&
                       settings.ContainsKey("FromEmail");
            }
            catch
            {
                return false;
            }
        }
    }
}
