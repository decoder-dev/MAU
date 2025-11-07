using NetworkMonitoringSystem.Core.Data;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Сервис для управления учетными данными устройств
    /// </summary>
    public class CredentialService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _credentialsPath;

        public CredentialService(ApplicationDbContext context)
        {
            _context = context;
            _credentialsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NetworkMonitoringSystem",
                "Credentials");

            if (!Directory.Exists(_credentialsPath))
                Directory.CreateDirectory(_credentialsPath);
        }

        /// <summary>
        /// Сохранить учетные данные устройства
        /// </summary>
        public async Task<bool> SaveDeviceCredentialsAsync(int deviceId, string username, string password)
        {
            try
            {
                var device = await _context.NetworkDevices.FindAsync(deviceId);
                if (device == null)
                    return false;

                var encryptedPassword = EncryptPassword(password);
                var credentials = new DeviceCredentials
                {
                    DeviceId = deviceId,
                    Username = username,
                    EncryptedPassword = encryptedPassword,
                    UpdatedAt = DateTime.UtcNow
                };

                var filePath = Path.Combine(_credentialsPath, $"device_{deviceId}.json");
                var json = System.Text.Json.JsonSerializer.Serialize(credentials, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, json);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения учетных данных: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Получить учетные данные устройства
        /// </summary>
        public async Task<DeviceCredentials?> GetDeviceCredentialsAsync(int deviceId)
        {
            try
            {
                var filePath = Path.Combine(_credentialsPath, $"device_{deviceId}.json");
                if (!File.Exists(filePath))
                    return null;

                var json = await File.ReadAllTextAsync(filePath);
                var credentials = System.Text.Json.JsonSerializer.Deserialize<DeviceCredentials>(json);
                
                if (credentials != null)
                {
                    credentials.Password = DecryptPassword(credentials.EncryptedPassword);
                }

                return credentials;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения учетных данных: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Удалить учетные данные устройства
        /// </summary>
        public bool DeleteDeviceCredentials(int deviceId)
        {
            try
            {
                var filePath = Path.Combine(_credentialsPath, $"device_{deviceId}.json");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления учетных данных: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Получить все сохраненные учетные данные
        /// </summary>
        public async Task<List<DeviceCredentials>> GetAllCredentialsAsync()
        {
            var credentials = new List<DeviceCredentials>();

            try
            {
                if (!Directory.Exists(_credentialsPath))
                    return credentials;

                var files = Directory.GetFiles(_credentialsPath, "device_*.json");
                foreach (var file in files)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(file);
                        var cred = System.Text.Json.JsonSerializer.Deserialize<DeviceCredentials>(json);
                        if (cred != null)
                        {
                            cred.Password = DecryptPassword(cred.EncryptedPassword);
                            credentials.Add(cred);
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения всех учетных данных: {ex.Message}");
            }

            return credentials;
        }

        private string EncryptPassword(string password)
        {
            // Упрощенное шифрование (в продакшене использовать более надежное решение)
            var key = GetEncryptionKey();
            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            ms.Write(aes.IV, 0, aes.IV.Length);
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(password);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        private string DecryptPassword(string encryptedPassword)
        {
            try
            {
                var fullCipher = Convert.FromBase64String(encryptedPassword);
                var key = GetEncryptionKey();

                using var aes = Aes.Create();
                var iv = new byte[aes.BlockSize / 8];
                var cipher = new byte[fullCipher.Length - iv.Length];

                Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                aes.Key = key;
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor();
                using var ms = new MemoryStream(cipher);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);
                return sr.ReadToEnd();
            }
            catch
            {
                return string.Empty;
            }
        }

        private byte[] GetEncryptionKey()
        {
            // Генерируем ключ на основе машинного имени и имени пользователя
            var keySource = $"{Environment.MachineName}_{Environment.UserName}_{"NetworkMonitoringSystem"}";
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(keySource));
        }

        public class DeviceCredentials
        {
            public int DeviceId { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string EncryptedPassword { get; set; } = string.Empty;
            public DateTime UpdatedAt { get; set; }
        }
    }
}
