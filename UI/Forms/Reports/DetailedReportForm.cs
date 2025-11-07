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
    /// Детальный отчет
    /// </summary>
    public partial class DetailedReportForm : Form
    {
        private DataGridView dgvData;
        private Button btnRefresh;
        private Button btnGenerate;
        private Button btnExport;
        private TextBox txtReport;
        private ComboBox cmbReportType;
        private ReportService? _reportService;

        public DetailedReportForm()
        {
            InitializeComponent();
            _reportService = new ReportService(Program.DbContext);
        }

        private void InitializeComponent()
        {
            this.Text = "Детальный отчет";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilters = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(15) };
            var lblType = new Label { Text = "Тип отчета:", Location = new Point(15, 15), AutoSize = true };
            cmbReportType = new ComboBox { Location = new Point(95, 12), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbReportType.Items.AddRange(new[] { "Устройства", "Заявки", "Пользователи", "Активность" });
            cmbReportType.SelectedIndex = 0;
            btnGenerate = new Button { Text = "Сгенерировать", Location = new Point(305, 10), Width = 130 };
            btnGenerate.Click += BtnGenerate_Click;
            btnRefresh = new Button { Text = "Обновить", Location = new Point(445, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            btnExport = new Button { Text = "Экспорт", Location = new Point(555, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelFilters.Controls.AddRange(new Control[] { lblType, cmbReportType, btnGenerate, btnRefresh, btnExport });

            var splitContainer = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 300 };
            txtReport = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, Font = new Font("Consolas", 9) };
            splitContainer.Panel1.Controls.Add(txtReport);

            dgvData = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvData.Columns.Add("Item", "Элемент");
            dgvData.Columns.Add("Details", "Детали");
            splitContainer.Panel2.Controls.Add(dgvData);

            this.Controls.Add(splitContainer);
            this.Controls.Add(panelFilters);
        }

        private async void BtnGenerate_Click(object? sender, EventArgs e)
        {
            try
            {
                btnGenerate.Enabled = false;
                string report = "";
                switch (cmbReportType.SelectedIndex)
                {
                    case 0:
                        report = await _reportService!.GenerateDeviceStatusReportAsync();
                        break;
                    case 1:
                        report = await _reportService!.GenerateMaintenanceReportAsync();
                        break;
                    case 2:
                        report = await _reportService!.GenerateUserActivityReportAsync();
                        break;
                    case 3:
                        report = await _reportService!.GenerateStatisticsReportAsync();
                        break;
                }
                txtReport.Text = report;
                LoadData();
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Генерация отчета");
            }
            finally
            {
                btnGenerate.Enabled = true;
            }
        }

        private async void LoadData()
        {
            try
            {
                dgvData.Rows.Clear();
                switch (cmbReportType.SelectedIndex)
                {
                    case 0:
                        var devices = await DataService.GetDevicesAsync();
                        foreach (var device in devices.Take(50))
                            dgvData.Rows.Add(device.Name, $"IP: {device.IpAddress}, Статус: {device.Status}, Тип: {device.DeviceType}");
                        break;
                    case 1:
                        var requests = await DataService.GetMaintenanceRequestsAsync();
                        foreach (var request in requests.Take(50))
                            dgvData.Rows.Add(request.Title, $"Статус: {request.Status}, Приоритет: {request.Priority}, Создано: {request.CreatedAt:yyyy-MM-dd}");
                        break;
                    case 2:
                        var users = await DataService.GetUsersAsync();
                        foreach (var user in users)
                            dgvData.Rows.Add(user.Username, $"Роль: {user.Role}, Активен: {user.IsActive}, Email: {user.Email ?? "Нет"}");
                        break;
                    case 3:
                        var activities = await Program.DbContext.UserActivities.OrderByDescending(a => a.Timestamp).Take(50).ToListAsync();
                        foreach (var activity in activities)
                            dgvData.Rows.Add($"Действие: {activity.Action}", $"Время: {activity.Timestamp:yyyy-MM-dd HH:mm:ss}, Пользователь ID: {activity.UserId}");
                        break;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка данных");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "Текстовые файлы (*.txt)|*.txt|CSV файлы (*.csv)|*.csv", FileName = $"detailed_report_{DateTime.Now:yyyyMMdd}.txt" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (dialog.FilterIndex == 1)
                    File.WriteAllText(dialog.FileName, txtReport.Text, System.Text.Encoding.UTF8);
                else
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("Элемент;Детали");
                    foreach (DataGridViewRow row in dgvData.Rows)
                        sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value}");
                    File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                }
                ErrorHandler.ShowSuccess("Отчет экспортирован");
            }
        }
    }
}
