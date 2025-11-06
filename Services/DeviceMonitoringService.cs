using System.Net.NetworkInformation;
using System.Net;
using NetworkMonitoringSystem.Core.Data;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Сервис для мониторинга сетевых устройств
    /// </summary>
    public class DeviceMonitoringService
    {
        private Thread? _monitoringThread;
        private bool _isRunning = false;
        private readonly object _lockObject = new object();

        public DeviceMonitoringService(ApplicationDbContext context)
        {
            // Контекст больше не сохраняется, создается новый для каждой операции
            // Это предотвращает проблемы с disposed context и утечками памяти
        }

        public void Start()
        {
            if (_isRunning) return;
            
            _isRunning = true;
            _monitoringThread = new Thread(MonitoringLoop)
            {
                IsBackground = true,
                Name = "DeviceMonitoringThread"
            };
            _monitoringThread.Start();
        }

        public void Stop()
        {
            _isRunning = false;
            if (_monitoringThread != null && _monitoringThread.IsAlive)
            {
                _monitoringThread.Join(TimeSpan.FromSeconds(5));
                if (_monitoringThread.IsAlive)
                {
                    // Если поток не завершился, прерываем его
                    try
                    {
                        _monitoringThread.Interrupt();
                    }
                    catch { }
                }
            }
        }

        private async void MonitoringLoop()
        {
            while (_isRunning)
            {
                try
                {
                    await CheckDevicesAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка при проверке устройств: {ex.Message}");
                }

                // Проверяем каждые 5 минут
                for (int i = 0; i < 300 && _isRunning; i++) // 300 секунд = 5 минут
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private ApplicationDbContext CreateNewContext()
        {
            var connectionString = DatabaseConnectionService.GetServerConnectionString();
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseMySql(connectionString, serverVersion)
                .Options;
            return new ApplicationDbContext(options);
        }

        private async Task CheckDevicesAsync()
        {
            if (!_isRunning) return;
            
            try
            {
                // Создаем новый контекст для каждой проверки, чтобы избежать проблем с disposed context
                using var context = CreateNewContext();
                var devices = await context.NetworkDevices
                    .Where(d => d.Status != DeviceStatus.Maintenance)
                    .ToListAsync();

                foreach (var device in devices)
                {
                    var previousStatus = device.Status;
                    var isOnline = await PingDeviceAsync(device.IpAddress);

                    if (isOnline)
                    {
                        device.Status = DeviceStatus.Online;
                        device.LastSeen = DateTime.UtcNow;
                    }
                    else
                    {
                        if (previousStatus == DeviceStatus.Online)
                        {
                            device.Status = DeviceStatus.Offline;
                        }
                    }

                    if (previousStatus != device.Status)
                    {
                        var history = new DeviceStatusHistory
                        {
                            DeviceId = device.Id,
                            Status = device.Status,
                            Message = isOnline ? "Устройство доступно" : "Устройство недоступно",
                            Timestamp = DateTime.UtcNow
                        };
                        context.DeviceStatusHistory.Add(history);
                    }

                    device.UpdatedAt = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при проверке устройств: {ex.Message}");
            }
        }

        private async Task<bool> PingDeviceAsync(string ipAddress)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(ipAddress, 3000);
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

    }
}

