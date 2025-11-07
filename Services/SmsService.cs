using NetworkMonitoringSystem.Core.Data;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Сервис для отправки SMS уведомлений
    /// </summary>
    public class SmsService
    {
        private readonly ApplicationDbContext _context;

        public SmsService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Отправить SMS уведомление
        /// </summary>
        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                var settings = GetSmsSettings();
                if (string.IsNullOrEmpty(settings.ApiUrl) || string.IsNullOrEmpty(settings.ApiKey))
                {
                    System.Diagnostics.Debug.WriteLine("SMS настройки не сконфигурированы");
                    return false;
                }

                // Имитация отправки SMS (в реальном приложении здесь будет вызов API SMS-провайдера)
                // Например: Twilio, SMS.ru, или другой провайдер
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {settings.ApiKey}");

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("to", phoneNumber),
                    new KeyValuePair<string, string>("message", message)
                });

                var response = await httpClient.PostAsync(settings.ApiUrl, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка отправки SMS: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Отправить SMS уведомление об изменении статуса устройства
        /// </summary>
        public async Task SendDeviceStatusSmsAsync(NetworkDevice device, DeviceStatus oldStatus, DeviceStatus newStatus, string phoneNumber)
        {
            var message = $"Устройство {device.Name} изменило статус: {oldStatus} -> {newStatus}";
            await SendSmsAsync(phoneNumber, message);
        }

        /// <summary>
        /// Отправить критическое SMS уведомление
        /// </summary>
        public async Task SendCriticalSmsAsync(NetworkDevice device, string phoneNumber)
        {
            var message = $"КРИТИЧНО: Устройство {device.Name} (IP: {device.IpAddress}) недоступно!";
            await SendSmsAsync(phoneNumber, message);
        }

        /// <summary>
        /// Получить настройки SMS из конфигурации
        /// </summary>
        private SmsSettings GetSmsSettings()
        {
            var settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NetworkMonitoringSystem",
                "sms_settings.json");

            if (File.Exists(settingsPath))
            {
                try
                {
                    var json = File.ReadAllText(settingsPath);
                    return System.Text.Json.JsonSerializer.Deserialize<SmsSettings>(json) ?? new SmsSettings();
                }
                catch { }
            }

            return new SmsSettings();
        }

        private class SmsSettings
        {
            public string? ApiUrl { get; set; }
            public string? ApiKey { get; set; }
            public string? SenderName { get; set; }
        }
    }
}
