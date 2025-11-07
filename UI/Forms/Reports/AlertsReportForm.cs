using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Reports
{
    /// <summary>
    /// Отчет по алертам
    /// </summary>
    public partial class AlertsReportForm : Form
    {
        private DataGridView dgvData;
        private Button btnRefresh;
        private Button btnExport;
        private AlertService? _alertService;

        public AlertsReportForm()
        {
            InitializeComponent();
            _alertService = new AlertService(Program.DbContext);
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Отчет по алертам";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnRefresh = new Button { Text = "Обновить", Location = new Point(15, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            btnExport = new Button { Text = "Экспорт", Location = new Point(125, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelTop.Controls.AddRange(new Control[] { btnRefresh, btnExport });

            dgvData = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvData.Columns.Add("Device", "Устройство");
            dgvData.Columns.Add("AlertType", "Тип");
            dgvData.Columns.Add("Message", "Сообщение");
            dgvData.Columns.Add("Timestamp", "Время");
            dgvData.Columns.Add("Status", "Статус");

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
                var devices = await DataService.GetDevicesAsync();
                var statusHistory = await Program.DbContext.DeviceStatusHistory
                    .Where(h => h.Status == DeviceStatus.Offline || h.Status == DeviceStatus.Degraded)
                    .OrderByDescending(h => h.Timestamp)
                    .Take(200)
                    .ToListAsync();
                
                foreach (var history in statusHistory)
                {
                    var device = devices.FirstOrDefault(d => d.Id == history.DeviceId);
                    var alertType = history.Status == DeviceStatus.Offline ? "Критический" : "Предупреждение";
                    var message = $"Устройство {device?.Name ?? "Неизвестно"} имеет статус {history.Status}";
                    dgvData.Rows.Add(
                        device?.Name ?? "Неизвестно",
                        alertType,
                        message,
                        history.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                        "Активен"
                    );
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка отчета");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"alerts_report_{DateTime.Now:yyyyMMdd}.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Устройство;Тип;Сообщение;Время;Статус");
                foreach (DataGridViewRow row in dgvData.Rows)
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value};{row.Cells[3].Value};{row.Cells[4].Value}");
                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Отчет экспортирован");
            }
        }
    }
}
