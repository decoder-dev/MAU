using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки сетевого сканирования
    /// </summary>
    public partial class NetworkScanSettingsForm : Form
    {
        private NumericUpDown numTimeout;
        private NumericUpDown numThreads;
        private CheckBox chkPingOnly;
        private Button btnSave;

        public NetworkScanSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки сетевого сканирования";
            this.Size = new Size(500, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            var lblTimeout = new Label { Text = "Таймаут сканирования (мс):", Location = new Point(20, 20), AutoSize = true };
            numTimeout = new NumericUpDown { Location = new Point(200, 17), Width = 100, Minimum = 100, Maximum = 10000, Value = 1000 };
            var lblThreads = new Label { Text = "Параллельных потоков:", Location = new Point(20, 60), AutoSize = true };
            numThreads = new NumericUpDown { Location = new Point(170, 57), Width = 100, Minimum = 1, Maximum = 100, Value = 10 };
            chkPingOnly = new CheckBox { Text = "Только ping (быстрее)", Location = new Point(20, 100), Checked = false };
            btnSave = new Button { Text = "Сохранить", Location = new Point(200, 140), Width = 150 };
            btnSave.Click += BtnSave_Click;
            panel.Controls.AddRange(new Control[] { lblTimeout, numTimeout, lblThreads, numThreads, chkPingOnly, btnSave });
            this.Controls.Add(panel);
        }

        private void LoadSettings()
        {
            try
            {
                var config = new ConfigurationService(Program.DbContext);
                numTimeout.Value = config.GetValue<int>("Scan_Timeout", 1000);
                numThreads.Value = config.GetValue<int>("Scan_Threads", 10);
                chkPingOnly.Checked = config.GetValue<bool>("Scan_PingOnly", false);
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
                config.SetValue("Scan_Timeout", (int)numTimeout.Value);
                config.SetValue("Scan_Threads", (int)numThreads.Value);
                config.SetValue("Scan_PingOnly", chkPingOnly.Checked);
                ErrorHandler.ShowSuccess("Настройки сохранены");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение настроек");
            }
        }
    }
}
