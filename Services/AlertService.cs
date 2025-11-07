using NetworkMonitoringSystem.Core.Data;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Сервис для обработки алертов и уведомлений о статусах устройств
    /// </summary>
    public class AlertService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly SmsService _smsService;

        public AlertService(ApplicationDbContext context)
        {
            _context = context;
            _emailService = new EmailService(context);
            _smsService = new SmsService(context);
        }

        /// <summary>
        /// Создать алерт об изменении статуса устройства
        /// </summary>
        public async Task CreateDeviceStatusAlertAsync(NetworkDevice device, DeviceStatus oldStatus, DeviceStatus newStatus)
        {
            try
            {
                // Записываем в историю статусов
                var statusHistory = new DeviceStatusHistory
                {
                    DeviceId = device.Id,
                    Status = newStatus,
                    Timestamp = DateTime.UtcNow
                };

                _context.DeviceStatusHistory.Add(statusHistory);
                await _context.SaveChangesAsync();

                // Отправляем уведомления
                if (ShouldSendAlert(oldStatus, newStatus))
                {
                    await _emailService.SendDeviceStatusAlertAsync(device, oldStatus, newStatus);

                    // Если критический статус - отправляем SMS админам
                    if (newStatus == DeviceStatus.Offline)
                    {
                        var admins = await _context.Users
                            .Where(u => u.IsActive && u.Role == UserRole.Admin)
                            .ToListAsync();

                        foreach (var admin in admins)
                        {
                            // В реальном приложении здесь бы была проверка наличия телефона
                            // await _smsService.SendCriticalSmsAsync(device, admin.PhoneNumber);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания алерта: {ex.Message}");
            }
        }

        /// <summary>
        /// Создать критический алерт
        /// </summary>
        public async Task CreateCriticalAlertAsync(NetworkDevice device)
        {
            try
            {
                await _emailService.SendCriticalAlertAsync(device);

                var admins = await _context.Users
                    .Where(u => u.IsActive && u.Role == UserRole.Admin)
                    .ToListAsync();

                foreach (var admin in admins)
                {
                    // В реальном приложении здесь бы была проверка наличия телефона
                    // await _smsService.SendCriticalSmsAsync(device, admin.PhoneNumber);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания критического алерта: {ex.Message}");
            }
        }

        /// <summary>
        /// Получить все активные алерты
        /// </summary>
        public async Task<List<DeviceStatusHistory>> GetActiveAlertsAsync(TimeSpan? timeWindow = null)
        {
            var window = timeWindow ?? TimeSpan.FromHours(24);
            var threshold = DateTime.UtcNow - window;

            return await _context.DeviceStatusHistory
                .Include(h => h.Device)
                .Where(h => h.Timestamp >= threshold && 
                           (h.Status == DeviceStatus.Offline || h.Status == DeviceStatus.Degraded))
                .OrderByDescending(h => h.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Проверить, нужно ли отправлять алерт
        /// </summary>
        private bool ShouldSendAlert(DeviceStatus oldStatus, DeviceStatus newStatus)
        {
            // Отправляем алерт если:
            // - Статус изменился на Offline или Degraded
            // - Статус изменился с Offline на Online (восстановление)
            return (newStatus == DeviceStatus.Offline || newStatus == DeviceStatus.Degraded) ||
                   (oldStatus == DeviceStatus.Offline && newStatus == DeviceStatus.Online);
        }

        /// <summary>
        /// Получить статистику алертов за период
        /// </summary>
        public async Task<AlertStatistics> GetAlertStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            var alerts = await _context.DeviceStatusHistory
                .Where(h => h.Timestamp >= startDate && h.Timestamp <= endDate)
                .ToListAsync();

            return new AlertStatistics
            {
                TotalAlerts = alerts.Count,
                CriticalAlerts = alerts.Count(a => a.Status == DeviceStatus.Offline),
                WarningAlerts = alerts.Count(a => a.Status == DeviceStatus.Degraded),
                ResolvedAlerts = alerts.Count(a => a.Status == DeviceStatus.Online),
                StartDate = startDate,
                EndDate = endDate
            };
        }

        public class AlertStatistics
        {
            public int TotalAlerts { get; set; }
            public int CriticalAlerts { get; set; }
            public int WarningAlerts { get; set; }
            public int ResolvedAlerts { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }
    }
}
