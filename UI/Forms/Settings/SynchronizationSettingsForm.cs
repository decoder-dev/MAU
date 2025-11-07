using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки синхронизации
    /// </summary>
    public partial class SynchronizationSettingsForm : Form
    {
        private NumericUpDown numSyncInterval;
        private CheckBox chkAutoSync;
        private Button btnSave;

        public SynchronizationSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки синхронизации";
            this.Size = new Size(500, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            var lblInterval = new Label { Text = "Интервал синхронизации (мин):", Location = new Point(20, 20), AutoSize = true };
            numSyncInterval = new NumericUpDown { Location = new Point(210, 17), Width = 150, Minimum = 1, Maximum = 1440, Value = 60 };
            chkAutoSync = new CheckBox { Text = "Автоматическая синхронизация", Location = new Point(20, 60), Checked = true };
            btnSave = new Button { Text = "Сохранить", Location = new Point(200, 100), Width = 150 };
            btnSave.Click += BtnSave_Click;
            panel.Controls.AddRange(new Control[] { lblInterval, numSyncInterval, chkAutoSync, btnSave });
            this.Controls.Add(panel);
        }

        private void LoadSettings()
        {
            try
            {
                var config = new ConfigurationService(Program.DbContext);
                numSyncInterval.Value = config.GetValue<int>("SyncInterval", 60);
                chkAutoSync.Checked = config.GetValue<bool>("AutoSync", true);
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
                config.SetValue("SyncInterval", (int)numSyncInterval.Value);
                config.SetValue("AutoSync", chkAutoSync.Checked);
                ErrorHandler.ShowSuccess("Настройки сохранены");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение настроек");
            }
        }
    }
}
