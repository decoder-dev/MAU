using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using NetworkMonitoringSystem.Core.Data;

namespace NetworkMonitoringSystem.Services
{
    /// <summary>
    /// Сервис для управления настройками приложения
    /// </summary>
    public class ConfigurationService
    {
        private readonly ApplicationDbContext _context;
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NetworkMonitoringSystem",
            "config.json");

        private static Dictionary<string, object>? _cache;

        public ConfigurationService(ApplicationDbContext context)
        {
            _context = context;
            LoadConfig();
        }

        private void LoadConfig()
        {
            if (_cache != null) return;

            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    _cache = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
                }
                else
                {
                    _cache = new Dictionary<string, object>();
                }
            }
            catch
            {
                _cache = new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Получить значение настройки
        /// </summary>
        public T? GetValue<T>(string key, T? defaultValue = default)
        {
            LoadConfig();
            if (_cache!.TryGetValue(key, out var value))
            {
                if (value is JsonElement element)
                {
                    return JsonSerializer.Deserialize<T>(element.GetRawText());
                }
                return (T?)Convert.ChangeType(value, typeof(T));
            }
            return defaultValue;
        }

        /// <summary>
        /// Установить значение настройки
        /// </summary>
        public void SetValue<T>(string key, T value)
        {
            LoadConfig();
            _cache![key] = value!;
            SaveConfig();
        }

        private void SaveConfig()
        {
            try
            {
                var directory = Path.GetDirectoryName(ConfigPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var json = JsonSerializer.Serialize(_cache, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения конфигурации: {ex.Message}");
            }
        }

        /// <summary>
        /// Получить все настройки
        /// </summary>
        public Dictionary<string, object> GetAllSettings()
        {
            LoadConfig();
            return new Dictionary<string, object>(_cache!);
        }

        /// <summary>
        /// Сбросить настройки к значениям по умолчанию
        /// </summary>
        public void ResetToDefaults()
        {
            _cache = new Dictionary<string, object>();
            SaveConfig();
        }
    }
}
