using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using System.Text.Json;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Общие настройки системы
    /// </summary>
    public partial class GeneralSettingsForm : Form
    {
        private TextBox txtAppName;
        private NumericUpDown numCheckInterval;
        private CheckBox chkAutoSync;
        private CheckBox chkAutoBackup;
        private NumericUpDown numBackupInterval;
        private Button btnSave;
        private Button btnCancel;

        public GeneralSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Общие настройки";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var lblAppName = new Label { Text = "Название приложения:", Location = new Point(20, 20), AutoSize = true };
            txtAppName = new TextBox { Location = new Point(20, 45), Width = 400, Text = "Система мониторинга МАУ" };

            var lblCheckInterval = new Label { Text = "Интервал проверки (минут):", Location = new Point(20, 80), AutoSize = true };
            numCheckInterval = new NumericUpDown { Location = new Point(20, 105), Width = 100, Minimum = 1, Maximum = 60, Value = 5 };

            chkAutoSync = new CheckBox { Text = "Автоматическая синхронизация", Location = new Point(20, 140), Width = 250, Checked = true };
            chkAutoBackup = new CheckBox { Text = "Автоматическое резервное копирование", Location = new Point(20, 170), Width = 300, Checked = true };

            var lblBackupInterval = new Label { Text = "Интервал резервного копирования (часов):", Location = new Point(20, 200), AutoSize = true };
            numBackupInterval = new NumericUpDown { Location = new Point(20, 225), Width = 100, Minimum = 1, Maximum = 168, Value = 24 };

            btnSave = new Button { Text = "Сохранить", Location = new Point(380, 260), Width = 100, DialogResult = DialogResult.OK };
            btnSave.Click += BtnSave_Click;
            btnCancel = new Button { Text = "Отмена", Location = new Point(270, 260), Width = 100, DialogResult = DialogResult.Cancel };

            panel.Controls.AddRange(new Control[] 
            { 
                lblAppName, txtAppName, 
                lblCheckInterval, numCheckInterval,
                chkAutoSync, chkAutoBackup,
                lblBackupInterval, numBackupInterval,
                btnSave, btnCancel
            });

            this.Controls.Add(panel);
        }

        private void LoadSettings()
        {
            try
            {
                var settingsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "NetworkMonitoringSystem",
                    "settings.json");

                if (File.Exists(settingsPath))
                {
                    var json = File.ReadAllText(settingsPath);
                    var settings = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    if (settings != null)
                    {
                        if (settings.ContainsKey("AppName"))
                            txtAppName.Text = settings["AppName"]?.ToString() ?? txtAppName.Text;
                        if (settings.ContainsKey("CheckInterval"))
                            numCheckInterval.Value = Convert.ToDecimal(settings["CheckInterval"]);
                        if (settings.ContainsKey("AutoSync"))
                            chkAutoSync.Checked = Convert.ToBoolean(settings["AutoSync"]);
                        if (settings.ContainsKey("AutoBackup"))
                            chkAutoBackup.Checked = Convert.ToBoolean(settings["AutoBackup"]);
                        if (settings.ContainsKey("BackupInterval"))
                            numBackupInterval.Value = Convert.ToDecimal(settings["BackupInterval"]);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка настроек");
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                var settingsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "NetworkMonitoringSystem",
                    "settings.json");

                var dir = Path.GetDirectoryName(settingsPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var settings = new Dictionary<string, object>
                {
                    { "AppName", txtAppName.Text },
                    { "CheckInterval", numCheckInterval.Value },
                    { "AutoSync", chkAutoSync.Checked },
                    { "AutoBackup", chkAutoBackup.Checked },
                    { "BackupInterval", numBackupInterval.Value }
                };

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(settingsPath, json);
                ErrorHandler.ShowSuccess("Настройки сохранены");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение настроек");
            }
        }
    }
}
