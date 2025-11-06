using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetworkMonitoringSystem.Core.Models
{
    /// <summary>
    /// Заявка на обслуживание устройства
    /// </summary>
    [Table("MaintenanceRequests")]
    public class MaintenanceRequest
    {
        [Key]
        [Column("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("DeviceId")]
        public int DeviceId { get; set; }

        [Required]
        [Column("UserId")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Заголовок заявки обязателен")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "Заголовок должен быть от 3 до 500 символов")]
        [Column("Title", TypeName = "VARCHAR(500)")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        [Column("Description", TypeName = "TEXT")]
        public string? Description { get; set; }

        [Required]
        [Column("Status", TypeName = "TINYINT")]
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        [StringLength(200)]
        [Column("AssignedTo", TypeName = "VARCHAR(200)")]
        public string? AssignedTo { get; set; } // Имя исполнителя

        [StringLength(2000)]
        [Column("Resolution", TypeName = "TEXT")]
        public string? Resolution { get; set; } // Решение проблемы

        [Required]
        [Column("Priority", TypeName = "TINYINT")]
        [Range(1, 4, ErrorMessage = "Приоритет должен быть от 1 до 4")]
        public int Priority { get; set; } = 1; // 1 - Низкий, 2 - Средний, 3 - Высокий, 4 - Критический

        [Required]
        [Column("CreatedAt", TypeName = "DATETIME")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("CompletedAt", TypeName = "DATETIME")]
        public DateTime? CompletedAt { get; set; }

        [Column("UpdatedAt", TypeName = "DATETIME")]
        public DateTime? UpdatedAt { get; set; }

        // Навигационные свойства
        [ForeignKey("DeviceId")]
        public virtual NetworkDevice Device { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }

    /// <summary>
    /// Статусы заявок
    /// </summary>
    public enum RequestStatus : byte
    {
        Pending = 0,      // Ожидает
        InProgress = 1,   // В работе
        Completed = 2,    // Завершена
        Cancelled = 3     // Отменена
    }
}
