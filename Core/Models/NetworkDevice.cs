using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetworkMonitoringSystem.Core.Models
{
    /// <summary>
    /// Модель сетевого устройства
    /// </summary>
    [Table("NetworkDevices")]
    public class NetworkDevice
    {
        [Key]
        [Column("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Название устройства обязательно")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Название должно быть от 1 до 200 символов")]
        [Column("Name", TypeName = "VARCHAR(200)")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        [Column("Description", TypeName = "VARCHAR(500)")]
        public string? Description { get; set; }

        [Required]
        [Column("DeviceType", TypeName = "TINYINT")]
        public DeviceType DeviceType { get; set; } = DeviceType.Unknown;

        [Required(ErrorMessage = "IP адрес обязателен")]
        [StringLength(45)] // Поддержка IPv6
        [Column("IpAddress", TypeName = "VARCHAR(45)")]
        public string IpAddress { get; set; } = string.Empty;

        [StringLength(17)] // MAC адрес: 00:00:00:00:00:00
        [Column("MacAddress", TypeName = "VARCHAR(17)")]
        public string? MacAddress { get; set; }

        [StringLength(100)]
        [Column("Model", TypeName = "VARCHAR(100)")]
        public string? Model { get; set; }

        [StringLength(100)]
        [Column("Manufacturer", TypeName = "VARCHAR(100)")]
        public string? Manufacturer { get; set; }

        [StringLength(100)]
        [Column("SerialNumber", TypeName = "VARCHAR(100)")]
        public string? SerialNumber { get; set; }

        [StringLength(50)]
        [Column("FirmwareVersion", TypeName = "VARCHAR(50)")]
        public string? FirmwareVersion { get; set; }

        [StringLength(200)]
        [Column("Location", TypeName = "VARCHAR(200)")]
        public string? Location { get; set; }

        [StringLength(200)]
        [Column("Building", TypeName = "VARCHAR(200)")]
        public string? Building { get; set; }

        [StringLength(100)]
        [Column("Room", TypeName = "VARCHAR(100)")]
        public string? Room { get; set; }

        [Column("Latitude", TypeName = "DECIMAL(10, 8)")]
        public decimal? Latitude { get; set; }

        [Column("Longitude", TypeName = "DECIMAL(11, 8)")]
        public decimal? Longitude { get; set; }

        [Required]
        [Column("Status", TypeName = "TINYINT")]
        public DeviceStatus Status { get; set; } = DeviceStatus.Unknown;

        [Column("LastSeen", TypeName = "DATETIME")]
        public DateTime? LastSeen { get; set; }

        [Required]
        [Column("CreatedAt", TypeName = "DATETIME")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("UpdatedAt", TypeName = "DATETIME")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        [Column("OneCId", TypeName = "VARCHAR(50)")]
        public string? OneCId { get; set; } // ID для интеграции с 1С

        [StringLength(50)]
        [Column("OneCCode", TypeName = "VARCHAR(50)")]
        public string? OneCCode { get; set; } // Код в 1С

        [StringLength(100)]
        [Column("CreatedBy", TypeName = "VARCHAR(100)")]
        public string? CreatedBy { get; set; } // Кто создал запись

        [StringLength(100)]
        [Column("UpdatedBy", TypeName = "VARCHAR(100)")]
        public string? UpdatedBy { get; set; } // Кто обновил запись

        // Навигационные свойства
        public virtual ICollection<DeviceStatusHistory> StatusHistory { get; set; } = new List<DeviceStatusHistory>();
        public virtual ICollection<DeviceMetric> Metrics { get; set; } = new List<DeviceMetric>();
        public virtual ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();
    }

    /// <summary>
    /// Типы устройств
    /// </summary>
    public enum DeviceType : byte
    {
        Unknown = 0,
        Router = 1,        // Роутер
        Switch = 2,        // Коммутатор
        Phone = 3,         // Телефон (IP-телефон, VoIP)
        Modem = 4,         // Модем
        Computer = 5,      // Компьютер
        Server = 6,        // Сервер
        AccessPoint = 7,   // Точка доступа Wi-Fi
        Printer = 8,       // Принтер/МФУ
        Scanner = 9,       // Сканер
        Camera = 10,       // IP-камера
        Firewall = 11,     // Межсетевой экран
        Other = 99         // Другое
    }

    /// <summary>
    /// Статусы устройств
    /// </summary>
    public enum DeviceStatus : byte
    {
        Unknown = 0,
        Online = 1,
        Offline = 2,
        Degraded = 3,
        Maintenance = 4
    }
}
