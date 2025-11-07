using NetworkMonitoringSystem.Core.Data;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Общий сервис для управления уведомлениями
    /// </summary>
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly SmsService _smsService;
        private readonly AlertService _alertService;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
            _emailService = new EmailService(context);
            _smsService = new SmsService(context);
            _alertService = new AlertService(context);
        }

        /// <summary>
        /// Отправить уведомление о заявке на обслуживание
        /// </summary>
        public async Task NotifyMaintenanceRequestCreatedAsync(MaintenanceRequest request)
        {
            try
            {
                await _emailService.SendMaintenanceRequestNotificationAsync(request);

                var device = await _context.NetworkDevices.FindAsync(request.DeviceId);
                if (device != null)
                {
                    var admins = await _context.Users
                        .Where(u => u.IsActive && u.Role == UserRole.Admin && !string.IsNullOrEmpty(u.Email))
                        .ToListAsync();

                    foreach (var admin in admins)
                    {
                        var subject = $"Новая заявка на обслуживание: {request.Title}";
                        var body = $"Создана новая заявка на обслуживание устройства {device.Name}.\n\n" +
                                  $"Приоритет: {request.Priority}\n" +
                                  $"Описание: {request.Description ?? "Нет описания"}";
                        
                        await _emailService.SendEmailAsync(admin.Email!, subject, body);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка отправки уведомления о заявке: {ex.Message}");
            }
        }

        /// <summary>
        /// Отправить уведомление об изменении статуса заявки
        /// </summary>
        public async Task NotifyMaintenanceRequestStatusChangedAsync(MaintenanceRequest request, RequestStatus oldStatus)
        {
            try
            {
                var device = await _context.NetworkDevices.FindAsync(request.DeviceId);
                var user = await _context.Users.FindAsync(request.UserId);

                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    var subject = $"Изменение статуса заявки: {request.Title}";
                    var body = $"Статус заявки изменен: {oldStatus} -> {request.Status}\n\n" +
                              $"Устройство: {device?.Name ?? "Неизвестно"}\n" +
                              $"Исполнитель: {request.AssignedTo ?? "Не назначен"}";

                    if (request.Status == RequestStatus.Completed && !string.IsNullOrEmpty(request.Resolution))
                    {
                        body += $"\n\nРешение: {request.Resolution}";
                    }

                    await _emailService.SendEmailAsync(user.Email, subject, body);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка отправки уведомления об изменении статуса: {ex.Message}");
            }
        }

        /// <summary>
        /// Отправить уведомление о критическом событии
        /// </summary>
        public async Task NotifyCriticalEventAsync(NetworkDevice device, string message)
        {
            try
            {
                await _alertService.CreateCriticalAlertAsync(device);

                var admins = await _context.Users
                    .Where(u => u.IsActive && u.Role == UserRole.Admin && !string.IsNullOrEmpty(u.Email))
                    .ToListAsync();

                var subject = $"КРИТИЧЕСКОЕ СОБЫТИЕ: {device.Name}";
                var body = $"Критическое событие с устройством {device.Name} (IP: {device.IpAddress})\n\n" +
                          $"{message}\n\n" +
                          $"Требуется немедленное внимание!";

                foreach (var admin in admins)
                {
                    await _emailService.SendEmailAsync(admin.Email!, subject, body);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка отправки критического уведомления: {ex.Message}");
            }
        }

        /// <summary>
        /// Получить настройки уведомлений для пользователя
        /// </summary>
        public async Task<NotificationSettings> GetUserNotificationSettingsAsync(int userId)
        {
            // В реальном приложении здесь бы была таблица с настройками уведомлений
            // Пока возвращаем настройки по умолчанию
            return new NotificationSettings
            {
                EmailEnabled = true,
                SmsEnabled = false,
                CriticalAlertsOnly = false
            };
        }

        public class NotificationSettings
        {
            public bool EmailEnabled { get; set; }
            public bool SmsEnabled { get; set; }
            public bool CriticalAlertsOnly { get; set; }
        }
    }
}
