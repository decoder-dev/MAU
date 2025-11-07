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
    /// Заявки требующие внимания
    /// </summary>
    public partial class AttentionRequiredRequestsForm : Form
    {
        private DataGridView dgvRequests;
        private Button btnRefresh;

        public AttentionRequiredRequestsForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Заявки требующие внимания";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnRefresh = new Button { Text = "Обновить", Location = new Point(15, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            panelTop.Controls.Add(btnRefresh);

            dgvRequests = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvRequests.Columns.Add("Id", "ID");
            dgvRequests.Columns.Add("Title", "Название");
            dgvRequests.Columns.Add("Device", "Устройство");
            dgvRequests.Columns.Add("Priority", "Приоритет");
            dgvRequests.Columns.Add("Status", "Статус");
            dgvRequests.Columns.Add("CreatedAt", "Создано");
            dgvRequests.Columns.Add("Days", "Дней");

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
                var urgent = requests.Where(r => 
                    (r.Status == RequestStatus.Pending || r.Status == RequestStatus.InProgress) &&
                    (r.Priority >= 3 || (DateTime.UtcNow - r.CreatedAt).Days > 7)
                ).OrderByDescending(r => r.Priority).ThenBy(r => r.CreatedAt);

                dgvRequests.Rows.Clear();
                foreach (var request in urgent)
                {
                    var days = (DateTime.UtcNow - request.CreatedAt).Days;
                    dgvRequests.Rows.Add(request.Id, request.Title, request.Device?.Name ?? "-", request.Priority, request.Status, request.CreatedAt.ToString("dd.MM.yyyy"), days);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка заявок");
            }
        }
    }
}
