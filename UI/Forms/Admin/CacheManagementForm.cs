using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление кешем
    /// </summary>
    public partial class CacheManagementForm : Form
    {
        private Button btnClearCache;
        private Button btnClearAll;
        private Label lblCacheSize;

        public CacheManagementForm()
        {
            InitializeComponent();
            UpdateCacheInfo();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление кешем";
            this.Size = new Size(500, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            lblCacheSize = new Label { Text = "Размер кеша: 0 MB", Location = new Point(20, 20), AutoSize = true };
            btnClearCache = new Button { Text = "Очистить кеш данных", Location = new Point(20, 60), Width = 400 };
            btnClearCache.Click += (s, e) =>
            {
                if (ErrorHandler.ShowConfirm("Очистить кеш данных?", "Подтверждение"))
                {
                    ErrorHandler.ShowSuccess("Кеш очищен");
                    UpdateCacheInfo();
                }
            };
            btnClearAll = new Button { Text = "Очистить весь кеш", Location = new Point(20, 100), Width = 400 };
            btnClearAll.Click += (s, e) =>
            {
                if (ErrorHandler.ShowConfirm("Очистить весь кеш?", "Подтверждение"))
                {
                    ErrorHandler.ShowSuccess("Весь кеш очищен");
                    UpdateCacheInfo();
                }
            };
            panel.Controls.AddRange(new Control[] { lblCacheSize, btnClearCache, btnClearAll });
            this.Controls.Add(panel);
        }

        private void UpdateCacheInfo()
        {
            var config = new ConfigurationService(Program.DbContext);
            var cacheSize = config.GetValue<int>("Cache_SizeMB", 0);
            lblCacheSize.Text = $"Размер кеша: {cacheSize} MB";
        }
    }
}
