using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

namespace NetworkMonitoringSystem.Core.Models
{
    /// <summary>
    /// Модель пользователя системы
    /// </summary>
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Имя пользователя обязательно")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Имя пользователя должно быть от 3 до 50 символов")]
        [Column("Username", TypeName = "VARCHAR(50)")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [StringLength(32)] // MD5 хеш всегда 32 символа
        [Column("PasswordHash", TypeName = "CHAR(32)")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "ФИО обязательно")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "ФИО должно быть от 2 до 200 символов")]
        [Column("FullName", TypeName = "VARCHAR(200)")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Неверный формат email")]
        [StringLength(100)]
        [Column("Email", TypeName = "VARCHAR(100)")]
        public string? Email { get; set; }

        [Required]
        [Column("Role", TypeName = "TINYINT")]
        public UserRole Role { get; set; } = UserRole.User;

        [Required]
        [Column("IsActive", TypeName = "BIT")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("CreatedAt", TypeName = "DATETIME")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("LastLoginAt", TypeName = "DATETIME")]
        public DateTime? LastLoginAt { get; set; }

        [StringLength(100)]
        [Column("CreatedBy", TypeName = "VARCHAR(100)")]
        public string? CreatedBy { get; set; } // Кто создал пользователя

        // Навигационные свойства
        public virtual ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();
        public virtual ICollection<UserActivity> Activities { get; set; } = new List<UserActivity>();

        /// <summary>
        /// Хеширование пароля MD5
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Пароль не может быть пустым", nameof(password));

            using (var md5 = MD5.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = md5.ComputeHash(bytes);
                return Convert.ToHexString(hash);
            }
        }

        /// <summary>
        /// Проверка пароля
        /// </summary>
        public bool VerifyPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            return PasswordHash.Equals(HashPassword(password), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Установка нового пароля
        /// </summary>
        public void SetPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Пароль не может быть пустым", nameof(password));

            if (password.Length < 4)
                throw new ArgumentException("Пароль должен содержать минимум 4 символа", nameof(password));

            PasswordHash = HashPassword(password);
        }
    }

    /// <summary>
    /// Роли пользователей
    /// </summary>
    public enum UserRole : byte
    {
        User = 0,      // Обычный пользователь
        Admin = 1      // Администратор
    }
}
