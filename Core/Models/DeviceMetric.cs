using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetworkMonitoringSystem.Core.Models
{
    /// <summary>
    /// Метрики устройства (CPU, память, температура и т.д.)
    /// </summary>
    [Table("DeviceMetrics")]
    public class DeviceMetric
    {
        [Key]
        [Column("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("DeviceId")]
        public int DeviceId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("MetricType", TypeName = "VARCHAR(50)")]
        public string MetricType { get; set; } = string.Empty; // CPU, Memory, Temperature, Uptime, etc.

        [Column("Value", TypeName = "DECIMAL(18, 4)")]
        public decimal? Value { get; set; }

        [StringLength(20)]
        [Column("Unit", TypeName = "VARCHAR(20)")]
        public string? Unit { get; set; } // %, MB, °C, etc.

        [Required]
        [Column("Timestamp", TypeName = "DATETIME")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Column("AdditionalData", TypeName = "TEXT")]
        public string? AdditionalData { get; set; } // JSON с дополнительными данными

        // Навигационное свойство
        [ForeignKey("DeviceId")]
        public virtual NetworkDevice Device { get; set; } = null!;
    }
}
