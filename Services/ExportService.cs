using System.Text;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using NetworkMonitoringSystem.Core.Data;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Сервис экспорта данных
    /// </summary>
    public class ExportService
    {
        private readonly ApplicationDbContext _context;

        public ExportService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Экспорт устройств в CSV
        /// </summary>
        public async Task<string> ExportDevicesToCsvAsync()
        {
            var devices = await _context.NetworkDevices
                .OrderBy(d => d.Name)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Название;Тип;IP адрес;Статус;Местоположение;Здание;Помещение;Последний контакт");

            foreach (var device in devices)
            {
                sb.AppendLine($"{device.Name};{device.DeviceType};{device.IpAddress};{device.Status};" +
                             $"{device.Location ?? ""};{device.Building ?? ""};{device.Room ?? ""};" +
                             $"{device.LastSeen?.ToString("dd.MM.yyyy HH:mm") ?? "Никогда"}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Экспорт заявок в CSV
        /// </summary>
        public async Task<string> ExportRequestsToCsvAsync()
        {
            var requests = await _context.MaintenanceRequests
                .Include(r => r.Device)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("ID;Устройство;Заголовок;Статус;Приоритет;Создано;Исполнитель");

            foreach (var request in requests)
            {
                sb.AppendLine($"{request.Id};{request.Device.Name};{request.Title};{request.Status};" +
                             $"{request.Priority};{request.CreatedAt:dd.MM.yyyy HH:mm};{request.AssignedTo ?? "Не назначен"}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Экспорт статистики
        /// </summary>
        public async Task<string> ExportStatisticsAsync()
        {
            var total = await _context.NetworkDevices.CountAsync();
            var online = await _context.NetworkDevices.CountAsync(d => d.Status == DeviceStatus.Online);
            var offline = await _context.NetworkDevices.CountAsync(d => d.Status == DeviceStatus.Offline);
            var degraded = await _context.NetworkDevices.CountAsync(d => d.Status == DeviceStatus.Degraded);
            var maintenance = await _context.NetworkDevices.CountAsync(d => d.Status == DeviceStatus.Maintenance);

            var sb = new StringBuilder();
            sb.AppendLine("СТАТИСТИКА УСТРОЙСТВ");
            sb.AppendLine($"Дата экспорта: {DateTime.Now:dd.MM.yyyy HH:mm}");
            sb.AppendLine();
            sb.AppendLine($"Всего устройств: {total}");
            sb.AppendLine($"Online: {online}");
            sb.AppendLine($"Offline: {offline}");
            sb.AppendLine($"Degraded: {degraded}");
            sb.AppendLine($"Maintenance: {maintenance}");
            sb.AppendLine();

            // По типам
            sb.AppendLine("ПО ТИПАМ:");
            foreach (DeviceType type in Enum.GetValues(typeof(DeviceType)))
            {
                var count = await _context.NetworkDevices.CountAsync(d => d.DeviceType == type);
                if (count > 0)
                    sb.AppendLine($"{type}: {count}");
            }

            return sb.ToString();
        }
    }
}


