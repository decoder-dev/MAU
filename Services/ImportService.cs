using NetworkMonitoringSystem.Core.Data;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Сервис для импорта данных из различных источников
    /// </summary>
    public class ImportService
    {
        private readonly ApplicationDbContext _context;

        public ImportService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Импортировать устройства из CSV файла
        /// </summary>
        public async Task<ImportResult> ImportDevicesFromCsvAsync(string filePath, bool skipHeader = true)
        {
            var result = new ImportResult();
            
            try
            {
                var lines = await File.ReadAllLinesAsync(filePath, Encoding.UTF8);
                var startIndex = skipHeader ? 1 : 0;

                for (int i = startIndex; i < lines.Length; i++)
                {
                    try
                    {
                        var line = lines[i];
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        var parts = line.Split(';');
                        if (parts.Length < 3)
                            continue;

                        var device = new NetworkDevice
                        {
                            Name = parts[0].Trim(),
                            DeviceType = ParseDeviceType(parts.Length > 1 ? parts[1].Trim() : ""),
                            IpAddress = parts[2].Trim(),
                            Status = DeviceStatus.Unknown,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        if (parts.Length > 3)
                            device.Description = parts[3].Trim();
                        if (parts.Length > 4)
                            device.Location = parts[4].Trim();
                        if (parts.Length > 5)
                            device.Building = parts[5].Trim();
                        if (parts.Length > 6)
                            device.Room = parts[6].Trim();

                        // Проверяем, не существует ли уже устройство с таким IP
                        var existing = await _context.NetworkDevices
                            .FirstOrDefaultAsync(d => d.IpAddress == device.IpAddress);

                        if (existing == null)
                        {
                            _context.NetworkDevices.Add(device);
                            result.Imported++;
                        }
                        else
                        {
                            result.Skipped++;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Строка {i + 1}: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Ошибка импорта: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Импортировать устройства из JSON файла
        /// </summary>
        public async Task<ImportResult> ImportDevicesFromJsonAsync(string filePath)
        {
            var result = new ImportResult();

            try
            {
                var json = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                var devices = JsonSerializer.Deserialize<List<NetworkDevice>>(json);

                if (devices == null)
                {
                    result.Success = false;
                    result.Errors.Add("Не удалось прочитать JSON файл");
                    return result;
                }

                foreach (var device in devices)
                {
                    try
                    {
                        var existing = await _context.NetworkDevices
                            .FirstOrDefaultAsync(d => d.IpAddress == device.IpAddress);

                        if (existing == null)
                        {
                            device.Id = 0; // Сброс ID
                            device.CreatedAt = DateTime.UtcNow;
                            device.UpdatedAt = DateTime.UtcNow;
                            _context.NetworkDevices.Add(device);
                            result.Imported++;
                        }
                        else
                        {
                            result.Skipped++;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Ошибка импорта устройства {device.Name}: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Ошибка импорта: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Импортировать устройства из списка IP адресов
        /// </summary>
        public async Task<ImportResult> ImportDevicesFromIpListAsync(List<string> ipAddresses)
        {
            var result = new ImportResult();

            foreach (var ip in ipAddresses)
            {
                try
                {
                    var existing = await _context.NetworkDevices
                        .FirstOrDefaultAsync(d => d.IpAddress == ip);

                    if (existing == null)
                    {
                        var device = new NetworkDevice
                        {
                            Name = $"Устройство {ip}",
                            IpAddress = ip,
                            DeviceType = DeviceType.Unknown,
                            Status = DeviceStatus.Unknown,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.NetworkDevices.Add(device);
                        result.Imported++;
                    }
                    else
                    {
                        result.Skipped++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Ошибка импорта IP {ip}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            result.Success = true;

            return result;
        }

        private DeviceType ParseDeviceType(string typeStr)
        {
            if (Enum.TryParse<DeviceType>(typeStr, true, out var type))
                return type;

            // Попытка распознать по тексту
            return typeStr.ToLower() switch
            {
                var s when s.Contains("router") || s.Contains("роутер") => DeviceType.Router,
                var s when s.Contains("switch") || s.Contains("коммутатор") => DeviceType.Switch,
                var s when s.Contains("server") || s.Contains("сервер") => DeviceType.Server,
                var s when s.Contains("printer") || s.Contains("принтер") => DeviceType.Printer,
                var s when s.Contains("access") || s.Contains("точка доступа") => DeviceType.AccessPoint,
                _ => DeviceType.Unknown
            };
        }

        public class ImportResult
        {
            public bool Success { get; set; }
            public int Imported { get; set; }
            public int Skipped { get; set; }
            public List<string> Errors { get; set; } = new();
        }
    }
}
