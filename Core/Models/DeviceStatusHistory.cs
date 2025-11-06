using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetworkMonitoringSystem.Core.Models
{
    /// <summary>
    /// История изменений статуса устройства
    /// </summary>
    [Table("DeviceStatusHistory")]
    public class DeviceStatusHistory
    {
        [Key]
        [Column("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("DeviceId")]
        public int DeviceId { get; set; }

        [Required]
        [Column("Status", TypeName = "TINYINT")]
        public DeviceStatus Status { get; set; }

        [StringLength(500)]
        [Column("Message", TypeName = "VARCHAR(500)")]
        public string? Message { get; set; }

        [Required]
        [Column("Timestamp", TypeName = "DATETIME")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        [Column("ChangedBy", TypeName = "VARCHAR(100)")]
        public string? ChangedBy { get; set; } // Кто изменил статус

        // Навигационное свойство
        [ForeignKey("DeviceId")]
        public virtual NetworkDevice Device { get; set; } = null!;
    }
}
