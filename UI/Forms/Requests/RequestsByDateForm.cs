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
    /// Заявки по датам
    /// </summary>
    public partial class RequestsByDateForm : Form
    {
        private DataGridView dgvRequests;
        private DateTimePicker dtpDate;
        private Button btnRefresh;
        private Button btnExport;

        public RequestsByDateForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Заявки по датам";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lbl = new Label { Text = "Дата:", Location = new Point(15, 15), AutoSize = true };
            dtpDate = new DateTimePicker { Location = new Point(60, 12), Width = 150, Format = DateTimePickerFormat.Short };
            dtpDate.Value = DateTime.Now;
            dtpDate.ValueChanged += (s, e) => LoadData();
            btnRefresh = new Button { Text = "Обновить", Location = new Point(220, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            btnExport = new Button { Text = "Экспорт", Location = new Point(330, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelTop.Controls.AddRange(new Control[] { lbl, dtpDate, btnRefresh, btnExport });

            dgvRequests = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvRequests.Columns.Add("Id", "ID");
            dgvRequests.Columns.Add("Title", "Название");
            dgvRequests.Columns.Add("Device", "Устройство");
            dgvRequests.Columns.Add("Status", "Статус");
            dgvRequests.Columns.Add("CreatedAt", "Создано");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvRequests);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private async void LoadData()
        {
            try
            {
                var requests = await DataService.GetMaintenanceRequestsAsync();
                var date = dtpDate.Value.Date;
                var filtered = requests.Where(r => r.CreatedAt.Date == date).OrderByDescending(r => r.CreatedAt);

                dgvRequests.Rows.Clear();
                foreach (var request in filtered)
                {
                    dgvRequests.Rows.Add(request.Id, request.Title, request.Device?.Name ?? "-", request.Status, request.CreatedAt.ToString("dd.MM.yyyy HH:mm"));
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка заявок");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"requests_by_date_{dtpDate.Value:yyyyMMdd}.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("ID;Название;Устройство;Статус;Создано");
                foreach (DataGridViewRow row in dgvRequests.Rows)
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value};{row.Cells[3].Value};{row.Cells[4].Value}");
                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Данные экспортированы");
            }
        }
    }
}
