using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Reports
{
    /// <summary>
    /// Отчет по расписанию
    /// </summary>
    public partial class ScheduleReportForm : Form
    {
        private DataGridView dgvData;
        private Button btnRefresh;
        private Button btnExport;

        public ScheduleReportForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Отчет по расписанию";
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
            dgvData.Columns.Add("Task", "Задание");
            dgvData.Columns.Add("Frequency", "Частота");
            dgvData.Columns.Add("LastRun", "Последний запуск");
            dgvData.Columns.Add("NextRun", "Следующий запуск");
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
                var requests = await DataService.GetMaintenanceRequestsAsync();
                var scheduled = requests.Where(r => r.Status == RequestStatus.Pending || r.Status == RequestStatus.InProgress).ToList();
                foreach (var request in scheduled)
                {
                    var nextRun = request.CreatedAt.AddDays(7);
                    dgvData.Rows.Add(request.Title, "Еженедельно", request.CreatedAt.ToString("yyyy-MM-dd HH:mm"), nextRun.ToString("yyyy-MM-dd HH:mm"), request.Status.ToString());
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка отчета");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"schedule_report_{DateTime.Now:yyyyMMdd}.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Задание;Частота;Последний запуск;Следующий запуск;Статус");
                foreach (DataGridViewRow row in dgvData.Rows)
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value};{row.Cells[3].Value};{row.Cells[4].Value}");
                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Отчет экспортирован");
            }
        }
    }
}
