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
    /// Отчет по заявкам исполнителя
    /// </summary>
    public partial class AssigneeReportForm : Form
    {
        private DataGridView dgvRequests;
        private ComboBox cmbAssignee;
        private Button btnRefresh;
        private Button btnExport;

        public AssigneeReportForm()
        {
            InitializeComponent();
            LoadAssignees();
        }

        private void InitializeComponent()
        {
            this.Text = "Отчет по заявкам исполнителя";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lbl = new Label { Text = "Исполнитель:", Location = new Point(15, 15), AutoSize = true };
            cmbAssignee = new ComboBox { Location = new Point(95, 12), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbAssignee.Items.Add("Все");
            cmbAssignee.SelectedIndex = 0;
            cmbAssignee.SelectedIndexChanged += (s, e) => LoadData();
            btnRefresh = new Button { Text = "Обновить", Location = new Point(410, 10), Width = 100 };
            btnRefresh.Click += (s, e) => { LoadAssignees(); LoadData(); };
            btnExport = new Button { Text = "Экспорт", Location = new Point(520, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelTop.Controls.AddRange(new Control[] { lbl, cmbAssignee, btnRefresh, btnExport });

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
            this.Controls.Add(panelTop);
        }

        private async void LoadAssignees()
        {
            try
            {
                var requests = await DataService.GetMaintenanceRequestsAsync();
                var assignees = requests.Where(r => !string.IsNullOrEmpty(r.AssignedTo)).Select(r => r.AssignedTo!).Distinct().OrderBy(a => a);
                cmbAssignee.Items.Clear();
                cmbAssignee.Items.Add("Все");
                foreach (var assignee in assignees)
                    cmbAssignee.Items.Add(assignee);
                if (cmbAssignee.Items.Count > 0)
                    cmbAssignee.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка исполнителей");
            }
        }

        private async void LoadData()
        {
            try
            {
                var requests = await DataService.GetMaintenanceRequestsAsync();
                if (cmbAssignee.SelectedIndex > 0)
                {
                    var assignee = cmbAssignee.Text;
                    requests = requests.Where(r => r.AssignedTo == assignee).ToList();
                }
                dgvRequests.Rows.Clear();
                foreach (var request in requests.OrderByDescending(r => r.CreatedAt))
                {
                    dgvRequests.Rows.Add(request.Id, request.Title, request.Device?.Name ?? "-", request.Status, request.Priority, request.CreatedAt.ToString("dd.MM.yyyy HH:mm"));
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка заявок");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"assignee_requests_{DateTime.Now:yyyyMMdd}.csv" };
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
