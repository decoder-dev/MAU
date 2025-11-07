using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace NetworkMonitoringSystem.UI.Forms.Requests
{
    /// <summary>
    /// Список заявок на обслуживание
    /// </summary>
    public partial class MaintenanceRequestsListForm : Form
    {
        private DataGridView dgvRequests;
        private Button btnRefresh;
        private Button btnCreate;
        private Button btnEdit;
        private Button btnExport;
        private ComboBox cmbFilterStatus;
        private Label lblInfo;

        public MaintenanceRequestsListForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Список заявок на обслуживание";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilters = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lblStatus = new Label { Text = "Статус:", Location = new Point(15, 15), AutoSize = true };
            cmbFilterStatus = new ComboBox { Location = new Point(70, 12), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFilterStatus.Items.Add("Все");
            cmbFilterStatus.Items.AddRange(Enum.GetNames(typeof(RequestStatus)));
            cmbFilterStatus.SelectedIndex = 0;
            cmbFilterStatus.SelectedIndexChanged += (s, e) => LoadData();
            panelFilters.Controls.AddRange(new Control[] { lblStatus, cmbFilterStatus });

            var panelButtons = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnCreate = new Button { Text = "Создать заявку", Location = new Point(15, 10), Width = 120 };
            btnCreate.Click += BtnCreate_Click;
            btnEdit = new Button { Text = "Редактировать", Location = new Point(145, 10), Width = 120 };
            btnEdit.Click += BtnEdit_Click;
            btnRefresh = new Button { Text = "Обновить", Location = new Point(275, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            btnExport = new Button { Text = "Экспорт", Location = new Point(385, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelButtons.Controls.AddRange(new Control[] { btnCreate, btnEdit, btnRefresh, btnExport });

            dgvRequests = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            dgvRequests.Columns.Add("Id", "ID");
            dgvRequests.Columns.Add("Title", "Название");
            dgvRequests.Columns.Add("Device", "Устройство");
            dgvRequests.Columns.Add("Status", "Статус");
            dgvRequests.Columns.Add("Priority", "Приоритет");
            dgvRequests.Columns.Add("CreatedAt", "Создано");
            dgvRequests.DoubleClick += (s, e) => BtnEdit_Click(s, e);

            lblInfo = new Label { Dock = DockStyle.Bottom, Height = 30, Text = "Готово", Padding = new Padding(15, 5, 0, 0) };

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvRequests);
            panelMain.Controls.Add(lblInfo);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelButtons);
            this.Controls.Add(panelFilters);
        }

        private async void LoadData()
        {
            try
            {
                var requests = await DataService.GetMaintenanceRequestsAsync();
                if (cmbFilterStatus.SelectedIndex > 0)
                {
                    var selectedStatus = (RequestStatus)Enum.Parse(typeof(RequestStatus), cmbFilterStatus.Text);
                    requests = requests.Where(r => r.Status == selectedStatus).ToList();
                }
                dgvRequests.Rows.Clear();
                foreach (var request in requests.OrderByDescending(r => r.CreatedAt))
                {
                    dgvRequests.Rows.Add(request.Id, request.Title, request.Device?.Name ?? "-", request.Status.ToString(), request.Priority, request.CreatedAt.ToString("dd.MM.yyyy HH:mm"));
                }
                lblInfo.Text = $"Всего заявок: {requests.Count}";
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка заявок");
            }
        }

        private void BtnCreate_Click(object? sender, EventArgs e)
        {
            var form = new MaintenanceRequestForm();
            if (form.ShowDialog() == DialogResult.OK)
                LoadData();
        }

        private async void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (dgvRequests.SelectedRows.Count == 0)
            {
                ErrorHandler.ShowWarning("Выберите заявку");
                return;
            }
            var requestId = Convert.ToInt32(dgvRequests.SelectedRows[0].Cells[0].Value);
            var requests = await DataService.GetMaintenanceRequestsAsync();
            var request = requests.FirstOrDefault(r => r.Id == requestId);
            if (request != null)
            {
                var form = new MaintenanceRequestForm(request.Device, request);
                if (form.ShowDialog() == DialogResult.OK)
                    LoadData();
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"requests_{DateTime.Now:yyyyMMdd}.csv" };
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