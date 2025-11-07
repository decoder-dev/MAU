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
    /// Заявки без исполнителя
    /// </summary>
    public partial class BulkAssignForm : Form
    {
        private DataGridView dgvRequests;
        private Button btnRefresh;
        private Button btnAssignAll;

        public BulkAssignForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Заявки без исполнителя";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnRefresh = new Button { Text = "Обновить", Location = new Point(15, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            btnAssignAll = new Button { Text = "Назначить всех", Location = new Point(125, 10), Width = 130 };
            btnAssignAll.Click += BtnAssignAll_Click;
            panelTop.Controls.AddRange(new Control[] { btnRefresh, btnAssignAll });

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

        private async void LoadData()
        {
            try
            {
                var requests = await DataService.GetMaintenanceRequestsAsync();
                var unassigned = requests.Where(r => string.IsNullOrEmpty(r.AssignedTo) && r.Status != RequestStatus.Completed && r.Status != RequestStatus.Cancelled)
                    .OrderByDescending(r => r.Priority).ThenBy(r => r.CreatedAt);

                dgvRequests.Rows.Clear();
                foreach (var request in unassigned)
                {
                    dgvRequests.Rows.Add(request.Id, request.Title, request.Device?.Name ?? "-", request.Status, request.Priority, request.CreatedAt.ToString("dd.MM.yyyy HH:mm"));
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка заявок");
            }
        }

        private async void BtnAssignAll_Click(object? sender, EventArgs e)
        {
            if (!ErrorHandler.ShowConfirm("Назначить исполнителя на все заявки?", "Подтверждение"))
                return;

            var form = new AssignRequestForm();
            if (form.ShowDialog() == DialogResult.OK)
                LoadData();
        }
    }
}
