using NetworkMonitoringSystem.Core.Data;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Сервис для интеграции с внешними системами (1С, другие системы мониторинга)
    /// </summary>
    public class IntegrationService
    {
        private readonly ApplicationDbContext _context;

        public IntegrationService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Синхронизировать устройства с системой 1С
        /// </summary>
        public async Task<bool> SyncWithOneCAsync(string apiUrl, string apiKey)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                // Получаем данные из 1С
                var response = await httpClient.GetAsync($"{apiUrl}/api/devices");
                if (!response.IsSuccessStatusCode)
                    return false;

                var json = await response.Content.ReadAsStringAsync();
                var oneCDevices = JsonSerializer.Deserialize<List<OneCDevice>>(json);

                if (oneCDevices == null)
                    return false;

                // Синхронизируем с базой данных
                foreach (var oneCDevice in oneCDevices)
                {
                    var existing = await _context.NetworkDevices
                        .FirstOrDefaultAsync(d => d.OneCId == oneCDevice.Id || d.IpAddress == oneCDevice.IpAddress);

                    if (existing == null)
                    {
                        // Создаем новое устройство
                        var device = new NetworkDevice
                        {
                            Name = oneCDevice.Name,
                            IpAddress = oneCDevice.IpAddress,
                            OneCId = oneCDevice.Id,
                            OneCCode = oneCDevice.Code,
                            DeviceType = ParseDeviceType(oneCDevice.Type),
                            Location = oneCDevice.Location,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.NetworkDevices.Add(device);
                    }
                    else
                    {
                        // Обновляем существующее
                        existing.Name = oneCDevice.Name;
                        existing.OneCId = oneCDevice.Id;
                        existing.OneCCode = oneCDevice.Code;
                        existing.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка синхронизации с 1С: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Экспортировать устройства в формат для 1С
        /// </summary>
        public async Task<string> ExportToOneCAsync()
        {
            var devices = await _context.NetworkDevices
                .Where(d => d.Status != DeviceStatus.Maintenance)
                .ToListAsync();

            var oneCDevices = devices.Select(d => new OneCDevice
            {
                Id = d.OneCId ?? d.Id.ToString(),
                Code = d.OneCCode ?? d.Id.ToString(),
                Name = d.Name,
                IpAddress = d.IpAddress,
                Type = d.DeviceType.ToString(),
                Location = d.Location,
                Status = d.Status.ToString()
            }).ToList();

            return JsonSerializer.Serialize(oneCDevices, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        /// <summary>
        /// Интеграция с системой мониторинга (например, Zabbix, Nagios)
        /// </summary>
        public async Task<bool> SyncWithMonitoringSystemAsync(string apiUrl, string apiKey, string systemType)
        {
            try
            {
                // Базовая реализация - в реальном приложении здесь будет специфичная логика для каждого типа системы
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                // Получаем устройства из внешней системы
                var response = await httpClient.GetAsync($"{apiUrl}/api/devices");
                if (!response.IsSuccessStatusCode)
                    return false;

                var json = await response.Content.ReadAsStringAsync();
                // Парсинг зависит от формата конкретной системы мониторинга

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка синхронизации с системой мониторинга: {ex.Message}");
                return false;
            }
        }

        private DeviceType ParseDeviceType(string? typeStr)
        {
            if (string.IsNullOrEmpty(typeStr))
                return DeviceType.Unknown;

            if (Enum.TryParse<DeviceType>(typeStr, true, out var type))
                return type;

            return DeviceType.Unknown;
        }

        private class OneCDevice
        {
            public string Id { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string IpAddress { get; set; } = string.Empty;
            public string? Type { get; set; }
            public string? Location { get; set; }
            public string? Status { get; set; }
        }
    }
}
