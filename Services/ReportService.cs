using NetworkMonitoringSystem.Core.Data;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Сервис для генерации отчетов
    /// </summary>
    public class ReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Сгенерировать отчет о статусе устройств
        /// </summary>
        public async Task<string> GenerateDeviceStatusReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ОТЧЕТ О СТАТУСЕ УСТРОЙСТВ");
            sb.AppendLine($"Дата генерации: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine();

            var total = await _context.NetworkDevices.CountAsync();
            var online = await _context.NetworkDevices.CountAsync(d => d.Status == DeviceStatus.Online);
            var offline = await _context.NetworkDevices.CountAsync(d => d.Status == DeviceStatus.Offline);
            var degraded = await _context.NetworkDevices.CountAsync(d => d.Status == DeviceStatus.Degraded);
            var maintenance = await _context.NetworkDevices.CountAsync(d => d.Status == DeviceStatus.Maintenance);

            sb.AppendLine($"Всего устройств: {total}");
            sb.AppendLine($"Online: {online} ({100.0 * online / total:F1}%)");
            sb.AppendLine($"Offline: {offline} ({100.0 * offline / total:F1}%)");
            sb.AppendLine($"Degraded: {degraded} ({100.0 * degraded / total:F1}%)");
            sb.AppendLine($"Maintenance: {maintenance} ({100.0 * maintenance / total:F1}%)");
            sb.AppendLine();

            // По типам устройств
            sb.AppendLine("ПО ТИПАМ УСТРОЙСТВ:");
            foreach (DeviceType type in Enum.GetValues(typeof(DeviceType)))
            {
                var count = await _context.NetworkDevices.CountAsync(d => d.DeviceType == type);
                if (count > 0)
                {
                    var onlineCount = await _context.NetworkDevices
                        .CountAsync(d => d.DeviceType == type && d.Status == DeviceStatus.Online);
                    sb.AppendLine($"  {type}: {count} (Online: {onlineCount})");
                }
            }

            // Недоступные устройства
            var offlineDevices = await _context.NetworkDevices
                .Where(d => d.Status == DeviceStatus.Offline)
                .OrderBy(d => d.Name)
                .ToListAsync();

            if (offlineDevices.Any())
            {
                sb.AppendLine();
                sb.AppendLine("НЕДОСТУПНЫЕ УСТРОЙСТВА:");
                foreach (var device in offlineDevices)
                {
                    sb.AppendLine($"  - {device.Name} ({device.IpAddress})");
                    if (device.LastSeen.HasValue)
                        sb.AppendLine($"    Последний контакт: {device.LastSeen.Value:dd.MM.yyyy HH:mm:ss}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Сгенерировать отчет о заявках на обслуживание
        /// </summary>
        public async Task<string> GenerateMaintenanceReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ОТЧЕТ О ЗАЯВКАХ НА ОБСЛУЖИВАНИЕ");
            sb.AppendLine($"Дата генерации: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine();

            var query = _context.MaintenanceRequests.Include(r => r.Device).Include(r => r.User).AsQueryable();

            if (startDate.HasValue)
                query = query.Where(r => r.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(r => r.CreatedAt <= endDate.Value);

            var requests = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();

            var total = requests.Count;
            var pending = requests.Count(r => r.Status == RequestStatus.Pending);
            var inProgress = requests.Count(r => r.Status == RequestStatus.InProgress);
            var completed = requests.Count(r => r.Status == RequestStatus.Completed);
            var cancelled = requests.Count(r => r.Status == RequestStatus.Cancelled);

            sb.AppendLine($"Всего заявок: {total}");
            sb.AppendLine($"Ожидают: {pending}");
            sb.AppendLine($"В работе: {inProgress}");
            sb.AppendLine($"Завершены: {completed}");
            sb.AppendLine($"Отменены: {cancelled}");
            sb.AppendLine();

            // По приоритетам
            sb.AppendLine("ПО ПРИОРИТЕТАМ:");
            for (int i = 1; i <= 4; i++)
            {
                var count = requests.Count(r => r.Priority == i);
                if (count > 0)
                {
                    var priorityName = i switch
                    {
                        1 => "Низкий",
                        2 => "Средний",
                        3 => "Высокий",
                        4 => "Критический",
                        _ => "Неизвестно"
                    };
                    sb.AppendLine($"  {priorityName}: {count}");
                }
            }

            // Список заявок
            if (requests.Any())
            {
                sb.AppendLine();
                sb.AppendLine("СПИСОК ЗАЯВОК:");
                foreach (var request in requests.Take(100)) // Ограничиваем вывод
                {
                    sb.AppendLine($"  [{request.Id}] {request.Title}");
                    sb.AppendLine($"    Устройство: {request.Device?.Name ?? "Неизвестно"}");
                    sb.AppendLine($"    Статус: {request.Status}, Приоритет: {request.Priority}");
                    sb.AppendLine($"    Создано: {request.CreatedAt:dd.MM.yyyy HH:mm}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Сгенерировать отчет о деятельности пользователей
        /// </summary>
        public async Task<string> GenerateUserActivityReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ОТЧЕТ О ДЕЯТЕЛЬНОСТИ ПОЛЬЗОВАТЕЛЕЙ");
            sb.AppendLine($"Дата генерации: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine();

            var query = _context.UserActivities.Include(a => a.User).AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate.Value);

            var activities = await query.OrderByDescending(a => a.Timestamp).ToListAsync();

            var totalActivities = activities.Count;
            var uniqueUsers = activities.Select(a => a.UserId).Distinct().Count();

            sb.AppendLine($"Всего действий: {totalActivities}");
            sb.AppendLine($"Уникальных пользователей: {uniqueUsers}");
            sb.AppendLine();

            // По типам действий
            sb.AppendLine("ПО ТИПАМ ДЕЙСТВИЙ:");
            var actionsGrouped = activities.GroupBy(a => a.Action)
                .OrderByDescending(g => g.Count());

            foreach (var group in actionsGrouped)
            {
                sb.AppendLine($"  {group.Key}: {group.Count()}");
            }

            // Топ пользователей
            sb.AppendLine();
            sb.AppendLine("ТОП ПОЛЬЗОВАТЕЛЕЙ ПО АКТИВНОСТИ:");
            var usersGrouped = activities.GroupBy(a => a.User?.FullName ?? "Неизвестно")
                .OrderByDescending(g => g.Count())
                .Take(10);

            foreach (var group in usersGrouped)
            {
                sb.AppendLine($"  {group.Key}: {group.Count()} действий");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Сгенерировать статистический отчет
        /// </summary>
        public async Task<string> GenerateStatisticsReportAsync()
        {
            var sb = new StringBuilder();
            sb.AppendLine("СТАТИСТИЧЕСКИЙ ОТЧЕТ");
            sb.AppendLine($"Дата генерации: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine();

            // Общая статистика
            var totalDevices = await _context.NetworkDevices.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalRequests = await _context.MaintenanceRequests.CountAsync();
            var totalHistory = await _context.DeviceStatusHistory.CountAsync();

            sb.AppendLine("ОБЩАЯ СТАТИСТИКА:");
            sb.AppendLine($"  Устройств: {totalDevices}");
            sb.AppendLine($"  Пользователей: {totalUsers}");
            sb.AppendLine($"  Заявок на обслуживание: {totalRequests}");
            sb.AppendLine($"  Записей истории: {totalHistory}");
            sb.AppendLine();

            // Доступность
            var uptime = await CalculateUptimeAsync();
            sb.AppendLine($"ДОСТУПНОСТЬ:");
            sb.AppendLine($"  Средняя доступность: {uptime:F2}%");
            sb.AppendLine();

            // Самые проблемные устройства
            var problematicDevices = await _context.DeviceStatusHistory
                .Where(h => h.Status == DeviceStatus.Offline)
                .GroupBy(h => h.DeviceId)
                .Select(g => new { DeviceId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            if (problematicDevices.Any())
            {
                sb.AppendLine("САМЫЕ ПРОБЛЕМНЫЕ УСТРОЙСТВА:");
                foreach (var item in problematicDevices)
                {
                    var device = await _context.NetworkDevices.FindAsync(item.DeviceId);
                    if (device != null)
                        sb.AppendLine($"  {device.Name} ({device.IpAddress}): {item.Count} отказов");
                }
            }

            return sb.ToString();
        }

        private async Task<double> CalculateUptimeAsync()
        {
            var totalDevices = await _context.NetworkDevices.CountAsync();
            if (totalDevices == 0) return 0;

            var onlineDevices = await _context.NetworkDevices
                .CountAsync(d => d.Status == DeviceStatus.Online);

            return 100.0 * onlineDevices / totalDevices;
        }
    }
}
