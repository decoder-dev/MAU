using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки подключения
    /// </summary>
    public partial class ConnectionSettingsForm : Form
    {
        private NumericUpDown numTimeout;
        private NumericUpDown numRetries;
        private CheckBox chkAutoReconnect;
        private Button btnSave;

        public ConnectionSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки подключения";
            this.Size = new Size(500, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            var lblTimeout = new Label { Text = "Таймаут подключения (сек):", Location = new Point(20, 20), AutoSize = true };
            numTimeout = new NumericUpDown { Location = new Point(200, 17), Width = 100, Minimum = 1, Maximum = 300, Value = 30 };
            var lblRetries = new Label { Text = "Количество попыток:", Location = new Point(20, 60), AutoSize = true };
            numRetries = new NumericUpDown { Location = new Point(170, 57), Width = 100, Minimum = 1, Maximum = 10, Value = 3 };
            chkAutoReconnect = new CheckBox { Text = "Автоматическое переподключение", Location = new Point(20, 100), Checked = true };
            btnSave = new Button { Text = "Сохранить", Location = new Point(200, 140), Width = 150 };
            btnSave.Click += BtnSave_Click;
            panel.Controls.AddRange(new Control[] { lblTimeout, numTimeout, lblRetries, numRetries, chkAutoReconnect, btnSave });
            this.Controls.Add(panel);
        }

        private void LoadSettings()
        {
            try
            {
                var config = new ConfigurationService(Program.DbContext);
                numTimeout.Value = config.GetValue<int>("Connection_Timeout", 30);
                numRetries.Value = config.GetValue<int>("Connection_Retries", 3);
                chkAutoReconnect.Checked = config.GetValue<bool>("Connection_AutoReconnect", true);
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
                config.SetValue("Connection_Timeout", (int)numTimeout.Value);
                config.SetValue("Connection_Retries", (int)numRetries.Value);
                config.SetValue("Connection_AutoReconnect", chkAutoReconnect.Checked);
                ErrorHandler.ShowSuccess("Настройки сохранены");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение настроек");
            }
        }
    }
}
