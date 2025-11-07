using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Core.Data;

namespace NetworkMonitoringSystem.Helpers
{
    /// <summary>
    /// Вспомогательный класс для работы с SMS
    /// </summary>
    public static class SmsHelper
    {
        /// <summary>
        /// Отправить SMS уведомление
        /// </summary>
        public static async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                using var context = Program.DbContext;
                var smsService = new SmsService(context);
                return await smsService.SendSmsAsync(phoneNumber, message);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Отправить критическое SMS уведомление
        /// </summary>
        public static async Task SendCriticalSmsAsync(
            NetworkMonitoringSystem.Core.Models.NetworkDevice device,
            string phoneNumber)
        {
            try
            {
                using var context = Program.DbContext;
                var smsService = new SmsService(context);
                await smsService.SendCriticalSmsAsync(device, phoneNumber);
            }
            catch { }
        }

        /// <summary>
        /// Проверить валидность номера телефона
        /// </summary>
        public static bool ValidatePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Удаляем все символы кроме цифр
            var digits = phoneNumber.Where(char.IsDigit).ToArray();
            return digits.Length >= 10 && digits.Length <= 15;
        }

        /// <summary>
        /// Форматировать номер телефона
        /// </summary>
        public static string FormatPhoneNumber(string phoneNumber)
        {
            var digits = phoneNumber.Where(char.IsDigit).ToArray();
            if (digits.Length == 11 && digits[0] == '8')
            {
                // Заменяем 8 на +7
                return $"+7{new string(digits.Skip(1).ToArray())}";
            }
            return new string(digits);
        }
    }
}
