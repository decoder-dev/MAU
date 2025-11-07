using NetworkMonitoringSystem.Core.Data;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Сервис для сканирования сети и обнаружения устройств
    /// </summary>
    public class ScanService
    {
        private readonly ApplicationDbContext _context;

        public ScanService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Сканировать диапазон IP адресов
        /// </summary>
        public async Task<List<ScanResult>> ScanIpRangeAsync(string startIp, string endIp, int timeoutMs = 1000)
        {
            var results = new List<ScanResult>();
            var start = IPAddress.Parse(startIp);
            var end = IPAddress.Parse(endIp);

            var startBytes = start.GetAddressBytes();
            var endBytes = end.GetAddressBytes();

            // Простое сканирование - только для IPv4
            if (start.AddressFamily != AddressFamily.InterNetwork || 
                end.AddressFamily != AddressFamily.InterNetwork)
            {
                return results;
            }

            var tasks = new List<Task<ScanResult>>();
            
            for (int i = 0; i <= 255; i++) // Ограничиваем диапазон для безопасности
            {
                var ipBytes = new byte[4];
                Array.Copy(startBytes, ipBytes, 4);
                ipBytes[3] = (byte)i;

                var ip = new IPAddress(ipBytes);
                tasks.Add(ScanIpAsync(ip.ToString(), timeoutMs));
            }

            var scanResults = await Task.WhenAll(tasks);
            results.AddRange(scanResults.Where(r => r.IsOnline));

            return results;
        }

        /// <summary>
        /// Сканировать подсеть
        /// </summary>
        public async Task<List<ScanResult>> ScanSubnetAsync(string subnet, int timeoutMs = 1000)
        {
            // Определяем диапазон подсети
            var parts = subnet.Split('/');
            if (parts.Length != 2)
                return new List<ScanResult>();

            var baseIp = parts[0];
            var cidr = int.Parse(parts[1]);

            // Упрощенное сканирование - только /24 подсети
            if (cidr != 24)
                return new List<ScanResult>();

            var ipParts = baseIp.Split('.');
            var startIp = $"{ipParts[0]}.{ipParts[1]}.{ipParts[2]}.1";
            var endIp = $"{ipParts[0]}.{ipParts[1]}.{ipParts[2]}.254";

            return await ScanIpRangeAsync(startIp, endIp, timeoutMs);
        }

        /// <summary>
        /// Сканировать один IP адрес
        /// </summary>
        public async Task<ScanResult> ScanIpAsync(string ipAddress, int timeoutMs = 1000)
        {
            var result = new ScanResult
            {
                IpAddress = ipAddress,
                ScanTime = DateTime.UtcNow
            };

            try
            {
                // Ping проверка
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(ipAddress, timeoutMs);
                
                result.IsOnline = reply.Status == IPStatus.Success;
                result.ResponseTime = reply.RoundtripTime;

                if (result.IsOnline)
                {
                    // Попытка определить MAC адрес (требует дополнительных прав)
                    result.MacAddress = await GetMacAddressAsync(ipAddress);
                    
                    // Попытка определить имя хоста
                    try
                    {
                        var hostEntry = await Dns.GetHostEntryAsync(ipAddress);
                        result.HostName = hostEntry.HostName;
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Автоматически добавить обнаруженные устройства в базу
        /// </summary>
        public async Task<int> ImportScannedDevicesAsync(List<ScanResult> scanResults)
        {
            int importedCount = 0;

            foreach (var result in scanResults.Where(r => r.IsOnline))
            {
                // Проверяем, существует ли уже устройство с таким IP
                var existing = await _context.NetworkDevices
                    .FirstOrDefaultAsync(d => d.IpAddress == result.IpAddress);

                if (existing == null)
                {
                    var device = new NetworkDevice
                    {
                        Name = result.HostName ?? $"Устройство {result.IpAddress}",
                        IpAddress = result.IpAddress,
                        MacAddress = result.MacAddress,
                        Status = DeviceStatus.Online,
                        DeviceType = DeviceType.Unknown,
                        LastSeen = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.NetworkDevices.Add(device);
                    importedCount++;
                }
                else
                {
                    // Обновляем информацию об обнаруженном устройстве
                    existing.LastSeen = DateTime.UtcNow;
                    existing.Status = DeviceStatus.Online;
                    if (!string.IsNullOrEmpty(result.MacAddress) && string.IsNullOrEmpty(existing.MacAddress))
                    {
                        existing.MacAddress = result.MacAddress;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return importedCount;
        }

        /// <summary>
        /// Получить MAC адрес устройства (требует дополнительных прав)
        /// </summary>
        private async Task<string?> GetMacAddressAsync(string ipAddress)
        {
            try
            {
                // Это упрощенная реализация
                // В реальном приложении может потребоваться использование ARP таблицы
                // или специальных библиотек
                return null;
            }
            catch
            {
                return null;
            }
        }

        public class ScanResult
        {
            public string IpAddress { get; set; } = string.Empty;
            public bool IsOnline { get; set; }
            public long ResponseTime { get; set; }
            public string? HostName { get; set; }
            public string? MacAddress { get; set; }
            public string? ErrorMessage { get; set; }
            public DateTime ScanTime { get; set; }
        }
    }
}
