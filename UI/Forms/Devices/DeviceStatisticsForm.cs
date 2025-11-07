using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Devices
{
    /// <summary>
    /// Статистика устройств
    /// </summary>
    public class DeviceStatisticsForm : Form
    {
        private DataGridView dgvStats;
        private Button btnRefresh;
        private StatisticsService? _statisticsService;

        public DeviceStatisticsForm()
        {
            InitializeComponent();
            _statisticsService = new StatisticsService(Program.DbContext);
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Статистика устройств";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnRefresh = new Button { Text = "Обновить", Location = new Point(15, 10), Width = 120 };
            btnRefresh.Click += (s, e) => LoadData();
            panelTop.Controls.Add(btnRefresh);

            dgvStats = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvStats.Columns.Add("Parameter", "Параметр");
            dgvStats.Columns.Add("Value", "Значение");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvStats);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private async void LoadData()
        {
            try
            {
                var stats = await _statisticsService!.GetDeviceStatisticsAsync();
                dgvStats.Rows.Clear();
                dgvStats.Rows.Add("Всего устройств", stats.Total);
                foreach (var item in stats.ByStatus)
                    dgvStats.Rows.Add($"Статус: {item.Key}", item.Value);
                foreach (var item in stats.ByType)
                    dgvStats.Rows.Add($"Тип: {item.Key}", item.Value);
                foreach (var item in stats.ByLocation)
                    dgvStats.Rows.Add($"Местоположение: {item.Key}", item.Value);
                foreach (var item in stats.ByBuilding)
                    dgvStats.Rows.Add($"Здание: {item.Key}", item.Value);
                dgvStats.Rows.Add("Процент онлайн", $"{stats.OnlinePercentage:F1}%");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка статистики");
            }
        }
    }
}
