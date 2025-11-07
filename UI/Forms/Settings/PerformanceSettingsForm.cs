using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки производительности
    /// </summary>
    public partial class PerformanceSettingsForm : Form
    {
        private NumericUpDown numCacheSize;
        private CheckBox chkUseCache;
        private NumericUpDown numThreads;
        private Button btnSave;

        public PerformanceSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки производительности";
            this.Size = new Size(500, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            var lblCache = new Label { Text = "Размер кеша (MB):", Location = new Point(20, 20), AutoSize = true };
            numCacheSize = new NumericUpDown { Location = new Point(160, 17), Width = 100, Minimum = 0, Maximum = 2048, Value = 256 };
            chkUseCache = new CheckBox { Text = "Использовать кеш", Location = new Point(20, 60), Checked = true };
            var lblThreads = new Label { Text = "Количество потоков:", Location = new Point(20, 100), AutoSize = true };
            numThreads = new NumericUpDown { Location = new Point(170, 97), Width = 100, Minimum = 1, Maximum = Environment.ProcessorCount, Value = Environment.ProcessorCount };
            btnSave = new Button { Text = "Сохранить", Location = new Point(200, 140), Width = 150 };
            btnSave.Click += BtnSave_Click;
            panel.Controls.AddRange(new Control[] { lblCache, numCacheSize, chkUseCache, lblThreads, numThreads, btnSave });
            this.Controls.Add(panel);
        }

        private void LoadSettings()
        {
            try
            {
                var config = new ConfigurationService(Program.DbContext);
                numCacheSize.Value = config.GetValue<int>("Performance_CacheSizeMB", 256);
                chkUseCache.Checked = config.GetValue<bool>("Performance_UseCache", true);
                numThreads.Value = config.GetValue<int>("Performance_Threads", Environment.ProcessorCount);
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
                config.SetValue("Performance_CacheSizeMB", (int)numCacheSize.Value);
                config.SetValue("Performance_UseCache", chkUseCache.Checked);
                config.SetValue("Performance_Threads", (int)numThreads.Value);
                ErrorHandler.ShowSuccess("Настройки сохранены");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение настроек");
            }
        }
    }
}
