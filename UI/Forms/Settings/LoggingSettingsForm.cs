using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки логов
    /// </summary>
    public partial class LoggingSettingsForm : Form
    {
        private ComboBox cmbLogLevel;
        private NumericUpDown numMaxLogSize;
        private NumericUpDown numLogRetention;
        private Button btnSave;

        public LoggingSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки логов";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            var lblLevel = new Label { Text = "Уровень логирования:", Location = new Point(20, 20), AutoSize = true };
            cmbLogLevel = new ComboBox { Location = new Point(180, 17), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbLogLevel.Items.AddRange(new[] { "DEBUG", "INFO", "WARNING", "ERROR" });
            cmbLogLevel.SelectedIndex = 1;
            var lblMaxSize = new Label { Text = "Макс. размер лога (MB):", Location = new Point(20, 60), AutoSize = true };
            numMaxLogSize = new NumericUpDown { Location = new Point(180, 57), Width = 100, Minimum = 1, Maximum = 1000, Value = 100 };
            var lblRetention = new Label { Text = "Хранить логи (дней):", Location = new Point(20, 100), AutoSize = true };
            numLogRetention = new NumericUpDown { Location = new Point(180, 97), Width = 100, Minimum = 1, Maximum = 365, Value = 30 };
            btnSave = new Button { Text = "Сохранить", Location = new Point(200, 140), Width = 150 };
            btnSave.Click += BtnSave_Click;
            panel.Controls.AddRange(new Control[] { lblLevel, cmbLogLevel, lblMaxSize, numMaxLogSize, lblRetention, numLogRetention, btnSave });
            this.Controls.Add(panel);
        }

        private void LoadSettings()
        {
            try
            {
                var config = new ConfigurationService(Program.DbContext);
                var level = config.GetValue<string>("Logging_Level", "INFO");
                cmbLogLevel.SelectedItem = level;
                numMaxLogSize.Value = config.GetValue<int>("Logging_MaxSizeMB", 100);
                numLogRetention.Value = config.GetValue<int>("Logging_RetentionDays", 30);
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
                var config = new ConfigurationService(Program.DbContext);
                config.SetValue("Logging_Level", cmbLogLevel.SelectedItem?.ToString() ?? "INFO");
                config.SetValue("Logging_MaxSizeMB", (int)numMaxLogSize.Value);
                config.SetValue("Logging_RetentionDays", (int)numLogRetention.Value);
                ErrorHandler.ShowSuccess("Настройки сохранены");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение настроек");
            }
        }
    }
}
