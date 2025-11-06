using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetworkMonitoringSystem.Core.Models
{
    /// <summary>
    /// История активности пользователей
    /// </summary>
    [Table("UserActivities")]
    public class UserActivity
    {
        [Key]
        [Column("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("UserId")]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Action", TypeName = "VARCHAR(100)")]
        public string Action { get; set; } = string.Empty; // Login, Logout, DeviceCreate, DeviceUpdate, etc.

        [StringLength(500)]
        [Column("Description", TypeName = "VARCHAR(500)")]
        public string? Description { get; set; }

        [StringLength(50)]
        [Column("EntityType", TypeName = "VARCHAR(50)")]
        public string? EntityType { get; set; } // Device, User, Request, etc.

        [Column("EntityId")]
        public int? EntityId { get; set; } // ID сущности

        [Required]
        [Column("Timestamp", TypeName = "DATETIME")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [StringLength(45)]
        [Column("IpAddress", TypeName = "VARCHAR(45)")]
        public string? IpAddress { get; set; } // IP адрес пользователя

        // Навигационное свойство
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}

