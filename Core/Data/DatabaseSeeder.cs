using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace NetworkMonitoringSystem.Core.Data
{
    /// <summary>
    /// Инициализация базы данных начальными данными
    /// </summary>
    public static class DatabaseSeeder
    {
        /// <summary>
        /// Заполнение базы данных начальными данными
        /// </summary>
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            try
            {
                // Проверяем, есть ли уже данные
                if (await context.NetworkDevices.AnyAsync())
                    return;

                // Создаем тестового обычного пользователя (если нет)
                if (!await context.Users.AnyAsync(u => u.Username == "user"))
                {
                    var testUser = new User
                    {
                        Username = "user",
                        FullName = "Тестовый пользователь",
                        Email = "user@mau.ru",
                        Role = UserRole.User,
                        IsActive = true,
                        CreatedBy = "System"
                    };
                    testUser.SetPassword("user");
                    context.Users.Add(testUser);
                }

            // Создаем тестовые устройства для МАУ
            var devices = new List<NetworkDevice>
            {
                new NetworkDevice
                {
                    Name = "Главный роутер МАУ",
                    Description = "Основной маршрутизатор главного корпуса",
                    DeviceType = DeviceType.Router,
                    IpAddress = "192.168.1.1",
                    MacAddress = "00:1B:44:11:3A:B7",
                    Model = "Cisco ISR4331",
                    Manufacturer = "Cisco",
                    SerialNumber = "FDO12345678",
                    FirmwareVersion = "16.09.04",
                    Location = "Главный корпус",
                    Building = "Главный корпус",
                    Room = "301",
                    Latitude = 68.9585m,
                    Longitude = 33.0827m,
                    Status = DeviceStatus.Online,
                    LastSeen = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new NetworkDevice
                {
                    Name = "Коммутатор 1 этаж",
                    Description = "Коммутатор первого этажа главного корпуса",
                    DeviceType = DeviceType.Switch,
                    IpAddress = "192.168.1.10",
                    MacAddress = "00:1B:44:11:3A:B8",
                    Model = "Cisco Catalyst 2960",
                    Manufacturer = "Cisco",
                    Location = "Главный корпус",
                    Building = "Главный корпус",
                    Room = "101",
                    Latitude = 68.9586m,
                    Longitude = 33.0828m,
                    Status = DeviceStatus.Online,
                    LastSeen = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new NetworkDevice
                {
                    Name = "Сервер баз данных",
                    Description = "Основной сервер для баз данных университета",
                    DeviceType = DeviceType.Server,
                    IpAddress = "192.168.1.100",
                    MacAddress = "00:1B:44:11:3A:B9",
                    Model = "Dell PowerEdge R740",
                    Manufacturer = "Dell",
                    Location = "Главный корпус",
                    Building = "Главный корпус",
                    Room = "401",
                    Latitude = 68.9587m,
                    Longitude = 33.0829m,
                    Status = DeviceStatus.Online,
                    LastSeen = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new NetworkDevice
                {
                    Name = "IP-телефон ректората",
                    Description = "IP-телефон в приемной ректора",
                    DeviceType = DeviceType.Phone,
                    IpAddress = "192.168.1.50",
                    MacAddress = "00:1B:44:11:3A:BA",
                    Model = "Cisco IP Phone 7962",
                    Manufacturer = "Cisco",
                    Location = "Главный корпус",
                    Building = "Главный корпус",
                    Room = "201",
                    Latitude = 68.9588m,
                    Longitude = 33.0830m,
                    Status = DeviceStatus.Online,
                    LastSeen = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new NetworkDevice
                {
                    Name = "Рабочая станция администратора",
                    Description = "Компьютер системного администратора",
                    DeviceType = DeviceType.Computer,
                    IpAddress = "192.168.1.25",
                    MacAddress = "00:1B:44:11:3A:BB",
                    Model = "HP EliteDesk 800",
                    Manufacturer = "HP",
                    Location = "Главный корпус",
                    Building = "Главный корпус",
                    Room = "105",
                    Latitude = 68.9589m,
                    Longitude = 33.0831m,
                    Status = DeviceStatus.Online,
                    LastSeen = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new NetworkDevice
                {
                    Name = "Wi-Fi точка доступа 1",
                    Description = "Точка доступа Wi-Fi в главном холле",
                    DeviceType = DeviceType.AccessPoint,
                    IpAddress = "192.168.1.200",
                    MacAddress = "00:1B:44:11:3A:BC",
                    Model = "Cisco Aironet 2802",
                    Manufacturer = "Cisco",
                    Location = "Главный корпус",
                    Building = "Главный корпус",
                    Room = "Холл 1 этаж",
                    Latitude = 68.9590m,
                    Longitude = 33.0832m,
                    Status = DeviceStatus.Online,
                    LastSeen = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new NetworkDevice
                {
                    Name = "Принтер бухгалтерии",
                    Description = "МФУ в отделе бухгалтерии",
                    DeviceType = DeviceType.Printer,
                    IpAddress = "192.168.1.150",
                    MacAddress = "00:1B:44:11:3A:BD",
                    Model = "HP LaserJet Pro M404",
                    Manufacturer = "HP",
                    Location = "Главный корпус",
                    Building = "Главный корпус",
                    Room = "205",
                    Latitude = 68.9591m,
                    Longitude = 33.0833m,
                    Status = DeviceStatus.Online,
                    LastSeen = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new NetworkDevice
                {
                    Name = "IP-камера вход",
                    Description = "Камера видеонаблюдения на главном входе",
                    DeviceType = DeviceType.Camera,
                    IpAddress = "192.168.1.220",
                    MacAddress = "00:1B:44:11:3A:BE",
                    Model = "Axis P3245",
                    Manufacturer = "Axis",
                    Location = "Главный корпус",
                    Building = "Главный корпус",
                    Room = "Вход",
                    Latitude = 68.9592m,
                    Longitude = 33.0834m,
                    Status = DeviceStatus.Online,
                    LastSeen = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.NetworkDevices.AddRange(devices);
            await context.SaveChangesAsync();

            // Добавляем историю статусов после сохранения (чтобы получить ID)
            foreach (var device in devices)
            {
                context.DeviceStatusHistory.Add(new DeviceStatusHistory
                {
                    DeviceId = device.Id,
                    Status = device.Status,
                    Message = "Устройство создано при инициализации системы",
                    Timestamp = DateTime.UtcNow,
                    ChangedBy = "System"
                });
            }

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при заполнении начальных данных: {ex.Message}");
            }
        }
    }
}

