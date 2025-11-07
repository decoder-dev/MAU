using NetworkMonitoringSystem.Core.Data;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Сервис для управления прошивками устройств
    /// </summary>
    public class FirmwareService
    {
        private readonly ApplicationDbContext _context;

        public FirmwareService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получить информацию о прошивках устройств
        /// </summary>
        public async Task<List<FirmwareInfo>> GetFirmwareInfoAsync()
        {
            var devices = await _context.NetworkDevices
                .Where(d => !string.IsNullOrEmpty(d.FirmwareVersion))
                .ToListAsync();

            return devices
                .GroupBy(d => new { d.DeviceType, d.Manufacturer, d.FirmwareVersion })
                .Select(g => new FirmwareInfo
                {
                    DeviceType = g.Key.DeviceType,
                    Manufacturer = g.Key.Manufacturer ?? "Неизвестно",
                    FirmwareVersion = g.Key.FirmwareVersion!,
                    DeviceCount = g.Count(),
                    Devices = g.Select(d => d.Name).ToList()
                })
                .OrderByDescending(f => f.DeviceCount)
                .ToList();
        }

        /// <summary>
        /// Получить устройства с устаревшей прошивкой
        /// </summary>
        public async Task<List<NetworkDevice>> GetDevicesWithOutdatedFirmwareAsync(string latestVersion)
        {
            return await _context.NetworkDevices
                .Where(d => !string.IsNullOrEmpty(d.FirmwareVersion) && 
                           d.FirmwareVersion != latestVersion)
                .ToListAsync();
        }

        /// <summary>
        /// Обновить версию прошивки устройства
        /// </summary>
        public async Task<bool> UpdateFirmwareVersionAsync(int deviceId, string newVersion)
        {
            try
            {
                var device = await _context.NetworkDevices.FindAsync(deviceId);
                if (device == null)
                    return false;

                device.FirmwareVersion = newVersion;
                device.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Получить статистику по прошивкам
        /// </summary>
        public async Task<FirmwareStatistics> GetFirmwareStatisticsAsync()
        {
            var devices = await _context.NetworkDevices.ToListAsync();
            
            return new FirmwareStatistics
            {
                TotalDevices = devices.Count,
                DevicesWithFirmware = devices.Count(d => !string.IsNullOrEmpty(d.FirmwareVersion)),
                UniqueFirmwareVersions = devices
                    .Where(d => !string.IsNullOrEmpty(d.FirmwareVersion))
                    .Select(d => d.FirmwareVersion!)
                    .Distinct()
                    .Count(),
                DevicesByType = devices
                    .GroupBy(d => d.DeviceType)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        public class FirmwareInfo
        {
            public DeviceType DeviceType { get; set; }
            public string Manufacturer { get; set; } = string.Empty;
            public string FirmwareVersion { get; set; } = string.Empty;
            public int DeviceCount { get; set; }
            public List<string> Devices { get; set; } = new();
        }

        public class FirmwareStatistics
        {
            public int TotalDevices { get; set; }
            public int DevicesWithFirmware { get; set; }
            public int UniqueFirmwareVersions { get; set; }
            public Dictionary<DeviceType, int> DevicesByType { get; set; } = new();
        }
    }
}
