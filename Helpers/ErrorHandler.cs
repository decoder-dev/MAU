using System;
using System.Windows.Forms;

namespace NetworkMonitoringSystem.Helpers
{
    public static class ErrorHandler
    {
        public static void ShowError(string message, string title = "Ошибка")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowWarning(string message, string title = "Предупреждение")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void ShowInfo(string message, string title = "Информация")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void ShowSuccess(string message, string title = "Успех")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static bool ShowConfirm(string message, string title = "Подтверждение")
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        public static void HandleException(Exception ex, string context = "")
        {
            var message = string.IsNullOrEmpty(context) 
                ? $"Произошла ошибка: {ex.Message}" 
                : $"Ошибка в {context}: {ex.Message}";
            
            ShowError(message, "Ошибка");
            
            // Логирование (можно добавить запись в файл)
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] {context}: {ex}");
        }
    }
}

