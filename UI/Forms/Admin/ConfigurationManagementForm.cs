using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление конфигурацией
    /// </summary>
    public partial class ConfigurationManagementForm : Form
    {
        private DataGridView dgvConfig;
        private Button btnSave;
        private Button btnReset;

        public ConfigurationManagementForm()
        {
            InitializeComponent();
            LoadConfig();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление конфигурацией";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnSave = new Button { Text = "Сохранить", Location = new Point(15, 10), Width = 100 };
            btnReset = new Button { Text = "Сбросить", Location = new Point(125, 10), Width = 100 };
            panelTop.Controls.AddRange(new Control[] { btnSave, btnReset });

            dgvConfig = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false };
            dgvConfig.Columns.Add("Key", "Параметр");
            dgvConfig.Columns.Add("Value", "Значение");
            dgvConfig.Columns.Add("Description", "Описание");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvConfig);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadConfig()
        {
            try
            {
                dgvConfig.Rows.Clear();
                dgvConfig.Rows.Add("SyncInterval", "60", "Интервал синхронизации (мин)");
                dgvConfig.Rows.Add("MonitorInterval", "30", "Интервал мониторинга (сек)");
                dgvConfig.Rows.Add("MaxLogSize", "100", "Макс. размер лога (MB)");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка конфигурации");
            }
        }
    }
}
