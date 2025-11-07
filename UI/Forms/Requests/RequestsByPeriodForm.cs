using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Requests
{
    /// <summary>
    /// Заявки за период
    /// </summary>
    public partial class RequestsByPeriodForm : Form
    {
        private DataGridView dgvRequests;
        private DateTimePicker dtpStart;
        private DateTimePicker dtpEnd;
        private Button btnRefresh;
        private Button btnExport;

        public RequestsByPeriodForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Заявки за период";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilters = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(15) };
            var lblStart = new Label { Text = "С:", Location = new Point(15, 15), AutoSize = true };
            dtpStart = new DateTimePicker { Location = new Point(40, 12), Width = 150, Format = DateTimePickerFormat.Short };
            dtpStart.Value = DateTime.Now.AddDays(-30);
            dtpStart.ValueChanged += (s, e) => LoadData();
            var lblEnd = new Label { Text = "По:", Location = new Point(200, 15), AutoSize = true };
            dtpEnd = new DateTimePicker { Location = new Point(230, 12), Width = 150, Format = DateTimePickerFormat.Short };
            dtpEnd.ValueChanged += (s, e) => LoadData();
            btnRefresh = new Button { Text = "Обновить", Location = new Point(400, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            btnExport = new Button { Text = "Экспорт", Location = new Point(510, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelFilters.Controls.AddRange(new Control[] { lblStart, dtpStart, lblEnd, dtpEnd, btnRefresh, btnExport });

            dgvRequests = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvRequests.Columns.Add("Id", "ID");
            dgvRequests.Columns.Add("Title", "Название");
            dgvRequests.Columns.Add("Device", "Устройство");
            dgvRequests.Columns.Add("Status", "Статус");
            dgvRequests.Columns.Add("Priority", "Приоритет");
            dgvRequests.Columns.Add("CreatedAt", "Создано");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvRequests);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelFilters);
        }

        private async void LoadData()
        {
            try
            {
                var requests = await DataService.GetMaintenanceRequestsAsync();
                var filtered = requests.Where(r => r.CreatedAt >= dtpStart.Value && r.CreatedAt <= dtpEnd.Value.AddDays(1)).ToList();
                dgvRequests.Rows.Clear();
                foreach (var request in filtered.OrderByDescending(r => r.CreatedAt))
                {
                    dgvRequests.Rows.Add(request.Id, request.Title, request.Device?.Name ?? "-", request.Status.ToString(), request.Priority, request.CreatedAt.ToString("dd.MM.yyyy HH:mm"));
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка заявок");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"requests_period_{DateTime.Now:yyyyMMdd}.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("ID;Название;Устройство;Статус;Приоритет;Создано");
                foreach (DataGridViewRow row in dgvRequests.Rows)
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value};{row.Cells[3].Value};{row.Cells[4].Value};{row.Cells[5].Value}");
                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Данные экспортированы");
            }
        }
    }
}
