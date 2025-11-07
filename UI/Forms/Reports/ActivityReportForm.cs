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
    /// Отчет по активности
    /// </summary>
    public partial class ActivityReportForm : Form
    {
        private DataGridView dgvData;
        private Button btnRefresh;
        private Button btnExport;
        private DateTimePicker dtpStart;
        private DateTimePicker dtpEnd;
        private ReportService? _reportService;

        public ActivityReportForm()
        {
            InitializeComponent();
            _reportService = new ReportService(Program.DbContext);
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Отчет по активности";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilters = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lblStart = new Label { Text = "С:", Location = new Point(15, 15), AutoSize = true };
            dtpStart = new DateTimePicker { Location = new Point(40, 12), Width = 150, Format = DateTimePickerFormat.Short };
            dtpStart.Value = DateTime.Now.AddDays(-7);
            dtpStart.ValueChanged += (s, e) => LoadData();
            var lblEnd = new Label { Text = "По:", Location = new Point(200, 15), AutoSize = true };
            dtpEnd = new DateTimePicker { Location = new Point(230, 12), Width = 150, Format = DateTimePickerFormat.Short };
            dtpEnd.ValueChanged += (s, e) => LoadData();
            btnRefresh = new Button { Text = "Обновить", Location = new Point(400, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            btnExport = new Button { Text = "Экспорт", Location = new Point(510, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelFilters.Controls.AddRange(new Control[] { lblStart, dtpStart, lblEnd, dtpEnd, btnRefresh, btnExport });

            dgvData = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvData.Columns.Add("User", "Пользователь");
            dgvData.Columns.Add("Action", "Действие");
            dgvData.Columns.Add("Count", "Количество");
            dgvData.Columns.Add("LastActivity", "Последняя активность");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvData);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelFilters);
        }

        private async void LoadData()
        {
            try
            {
                var report = await _reportService!.GenerateUserActivityReportAsync(dtpStart.Value, dtpEnd.Value);
                dgvData.Rows.Clear();
                
                if (report != null && !string.IsNullOrEmpty(report))
                {
                    var activities = await Program.DbContext.UserActivities
                        .Where(a => a.Timestamp >= dtpStart.Value && a.Timestamp <= dtpEnd.Value)
                        .GroupBy(a => new { a.UserId, a.Action })
                        .Select(g => new { UserId = g.Key.UserId, Action = g.Key.Action, Count = g.Count(), LastActivity = g.Max(a => a.Timestamp) })
                        .ToListAsync();
                    
                    var users = await DataService.GetUsersAsync();
                    foreach (var activity in activities)
                    {
                        var user = users.FirstOrDefault(u => u.Id == activity.UserId);
                        dgvData.Rows.Add(
                            user?.Username ?? "Неизвестно",
                            activity.Action,
                            activity.Count,
                            activity.LastActivity.ToString("yyyy-MM-dd HH:mm:ss")
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка отчета");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"activity_report_{DateTime.Now:yyyyMMdd}.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Пользователь;Действие;Количество;Последняя активность");
                foreach (DataGridViewRow row in dgvData.Rows)
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value};{row.Cells[3].Value}");
                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Отчет экспортирован");
            }
        }
    }
}
