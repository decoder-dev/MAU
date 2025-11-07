using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки мониторинга
    /// </summary>
    public partial class MonitoringSettingsForm : Form
    {
        private NumericUpDown numInterval;
        private CheckBox chkAutoScan;
        private Button btnSave;

        public MonitoringSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки мониторинга";
            this.Size = new Size(500, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            var lblInterval = new Label { Text = "Интервал мониторинга (сек):", Location = new Point(20, 20), AutoSize = true };
            numInterval = new NumericUpDown { Location = new Point(200, 17), Width = 150, Minimum = 10, Maximum = 3600, Value = 30 };
            chkAutoScan = new CheckBox { Text = "Автоматическое сканирование", Location = new Point(20, 60), Checked = true };
            btnSave = new Button { Text = "Сохранить", Location = new Point(200, 100), Width = 150 };
            btnSave.Click += BtnSave_Click;
            panel.Controls.AddRange(new Control[] { lblInterval, numInterval, chkAutoScan, btnSave });
            this.Controls.Add(panel);
        }

        private void LoadSettings()
        {
            try
            {
                var config = new ConfigurationService(Program.DbContext);
                numInterval.Value = config.GetValue<int>("MonitorInterval", 30);
                chkAutoScan.Checked = config.GetValue<bool>("AutoScan", true);
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
                config.SetValue("MonitorInterval", (int)numInterval.Value);
                config.SetValue("AutoScan", chkAutoScan.Checked);
                ErrorHandler.ShowSuccess("Настройки сохранены");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение настроек");
            }
        }
    }
}
