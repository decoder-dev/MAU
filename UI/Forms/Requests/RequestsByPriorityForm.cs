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
    /// Заявки по приоритету
    /// </summary>
    public partial class RequestsByPriorityForm : Form
    {
        private DataGridView dgvRequests;
        private ComboBox cmbPriority;
        private Button btnRefresh;
        private Button btnExport;

        public RequestsByPriorityForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Заявки по приоритету";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilters = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lblPriority = new Label { Text = "Приоритет:", Location = new Point(15, 15), AutoSize = true };
            cmbPriority = new ComboBox { Location = new Point(85, 12), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPriority.Items.AddRange(new[] { "Все", "1", "2", "3", "4" });
            cmbPriority.SelectedIndex = 0;
            cmbPriority.SelectedIndexChanged += (s, e) => LoadData();
            btnRefresh = new Button { Text = "Обновить", Location = new Point(295, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            btnExport = new Button { Text = "Экспорт", Location = new Point(405, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelFilters.Controls.AddRange(new Control[] { lblPriority, cmbPriority, btnRefresh, btnExport });

            dgvRequests = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvRequests.Columns.Add("Id", "ID");
            dgvRequests.Columns.Add("Title", "Название");
            dgvRequests.Columns.Add("Device", "Устройство");
            dgvRequests.Columns.Add("Status", "Статус");
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
                if (cmbPriority.SelectedIndex > 0)
                {
                    var priority = cmbPriority.SelectedIndex;
                    requests = requests.Where(r => r.Priority == priority).ToList();
                }
                dgvRequests.Rows.Clear();
                foreach (var request in requests.OrderByDescending(r => r.CreatedAt))
                {
                    dgvRequests.Rows.Add(request.Id, request.Title, request.Device?.Name ?? "-", request.Status.ToString(), request.CreatedAt.ToString("dd.MM.yyyy HH:mm"));
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка заявок");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"requests_by_priority_{DateTime.Now:yyyyMMdd}.csv" };
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
