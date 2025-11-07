using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Reports
{
    /// <summary>
    /// Отчет по аналитике
    /// </summary>
    public partial class AnalyticsReportForm : Form
    {
        private DataGridView dgvData;
        private Button btnRefresh;
        private Button btnExport;
        private AnalyticsService? _analyticsService;

        public AnalyticsReportForm()
        {
            InitializeComponent();
            _analyticsService = new AnalyticsService(Program.DbContext);
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Отчет по аналитике";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnRefresh = new Button { Text = "Обновить", Location = new Point(15, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            btnExport = new Button { Text = "Экспорт", Location = new Point(125, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelTop.Controls.AddRange(new Control[] { btnRefresh, btnExport });

            dgvData = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvData.Columns.Add("Metric", "Метрика");
            dgvData.Columns.Add("Value", "Значение");
            dgvData.Columns.Add("Trend", "Тенденция");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvData);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private async void LoadData()
        {
            try
            {
                dgvData.Rows.Clear();
                var analytics = await _analyticsService!.GetDeviceAnalyticsAsync();
                dgvData.Rows.Add("Всего устройств", analytics.TotalDevices, "Стабильно");
                dgvData.Rows.Add("Онлайн", analytics.OnlineDevices, "Стабильно");
                dgvData.Rows.Add("Офлайн", analytics.OfflineDevices, analytics.OfflineDevices > 0 ? "Ухудшение" : "Стабильно");
                dgvData.Rows.Add("Средний Uptime", $"{analytics.AverageUptime:F2}%", analytics.AverageUptime > 95 ? "Улучшение" : "Стабильно");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка отчета");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"analytics_report_{DateTime.Now:yyyyMMdd}.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Метрика;Значение;Тенденция");
                foreach (DataGridViewRow row in dgvData.Rows)
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value}");
                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Отчет экспортирован");
            }
        }
    }
}
