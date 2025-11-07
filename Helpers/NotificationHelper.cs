using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Core.Data;

namespace NetworkMonitoringSystem.Helpers
{
    /// <summary>
    /// Вспомогательный класс для работы с уведомлениями
    /// </summary>
    public static class NotificationHelper
    {
        /// <summary>
        /// Отправить уведомление о заявке на обслуживание
        /// </summary>
        public static async Task NotifyMaintenanceRequestAsync(
            NetworkMonitoringSystem.Core.Models.MaintenanceRequest request)
        {
            try
            {
                using var context = Program.DbContext;
                var notificationService = new NotificationService(context);
                await notificationService.NotifyMaintenanceRequestCreatedAsync(request);
            }
            catch { }
        }

        /// <summary>
        /// Отправить уведомление об изменении статуса заявки
        /// </summary>
        public static async Task NotifyMaintenanceRequestStatusChangedAsync(
            NetworkMonitoringSystem.Core.Models.MaintenanceRequest request,
            NetworkMonitoringSystem.Core.Models.RequestStatus oldStatus)
        {
            try
            {
                using var context = Program.DbContext;
                var notificationService = new NotificationService(context);
                await notificationService.NotifyMaintenanceRequestStatusChangedAsync(request, oldStatus);
            }
            catch { }
        }

        /// <summary>
        /// Отправить критическое уведомление
        /// </summary>
        public static async Task NotifyCriticalEventAsync(
            NetworkMonitoringSystem.Core.Models.NetworkDevice device,
            string message)
        {
            try
            {
                using var context = Program.DbContext;
                var notificationService = new NotificationService(context);
                await notificationService.NotifyCriticalEventAsync(device, message);
            }
            catch { }
        }

        /// <summary>
        /// Показать уведомление в системе Windows
        /// </summary>
        public static void ShowSystemNotification(string title, string message, System.Windows.Forms.ToolTipIcon icon = System.Windows.Forms.ToolTipIcon.Info)
        {
            // В Windows Forms можно использовать NotifyIcon для системных уведомлений
            // Здесь базовая реализация
            System.Diagnostics.Debug.WriteLine($"[{icon}] {title}: {message}");
        }
    }
}
