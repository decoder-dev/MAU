using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Devices
{
    /// <summary>
    /// Расписание обслуживания
    /// </summary>
    public partial class DeviceMaintenanceScheduleOverviewForm : Form
    {
        private DataGridView dgvSchedule;
        private Button btnRefresh;
        private Button btnCreate;
        private Button btnExport;
        private ScheduleService? _scheduleService;

        public DeviceMaintenanceScheduleOverviewForm()
        {
            InitializeComponent();
            _scheduleService = new ScheduleService(Program.DbContext);
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Расписание обслуживания";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelButtons = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnCreate = new Button { Text = "Создать задание", Location = new Point(15, 10), Width = 130 };
            btnCreate.Click += BtnCreate_Click;
            btnRefresh = new Button { Text = "Обновить", Location = new Point(155, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            btnExport = new Button { Text = "Экспорт", Location = new Point(265, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelButtons.Controls.AddRange(new Control[] { btnCreate, btnRefresh, btnExport });

            dgvSchedule = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvSchedule.Columns.Add("Device", "Устройство");
            dgvSchedule.Columns.Add("Task", "Задание");
            dgvSchedule.Columns.Add("NextRun", "Следующий запуск");
            dgvSchedule.Columns.Add("Frequency", "Частота");
            dgvSchedule.Columns.Add("Status", "Статус");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvSchedule);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelButtons);
        }

        private async void LoadData()
        {
            try
            {
                dgvSchedule.Rows.Clear();
                var requests = await DataService.GetMaintenanceRequestsAsync();
                var devices = await DataService.GetDevicesAsync();
                
                var scheduledRequests = requests
                    .Where(r => r.Status == RequestStatus.InProgress || r.Status == RequestStatus.Pending)
                    .OrderBy(r => r.CreatedAt);
                
                foreach (var request in scheduledRequests)
                {
                    var device = devices.FirstOrDefault(d => d.Id == request.DeviceId);
                    if (device != null)
                    {
                        var nextRun = request.CreatedAt.AddDays(7);
                        var frequency = "Еженедельно";
                        dgvSchedule.Rows.Add(
                            device.Name,
                            request.Title,
                            nextRun.ToString("yyyy-MM-dd HH:mm"),
                            frequency,
                            request.Status.ToString()
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка расписания");
            }
        }

        private void BtnCreate_Click(object? sender, EventArgs e)
        {
            ErrorHandler.ShowInfo("Функция создания задания будет реализована");
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"schedule_{DateTime.Now:yyyyMMdd}.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Устройство;Задание;Следующий запуск;Частота;Статус");
                foreach (DataGridViewRow row in dgvSchedule.Rows)
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value};{row.Cells[3].Value};{row.Cells[4].Value}");
                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Данные экспортированы");
            }
        }
    }
}
