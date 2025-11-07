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
    /// Просроченные заявки
    /// </summary>
    public partial class OverdueRequestsForm : Form
    {
        private DataGridView dgvRequests;
        private Button btnRefresh;
        private NumericUpDown numDays;

        public OverdueRequestsForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Просроченные заявки";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lbl = new Label { Text = "Дней просрочки:", Location = new Point(15, 15), AutoSize = true };
            numDays = new NumericUpDown { Location = new Point(130, 12), Width = 80, Minimum = 1, Maximum = 365, Value = 7 };
            numDays.ValueChanged += (s, e) => LoadData();
            btnRefresh = new Button { Text = "Обновить", Location = new Point(220, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            panelTop.Controls.AddRange(new Control[] { lbl, numDays, btnRefresh });

            dgvRequests = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvRequests.Columns.Add("Id", "ID");
            dgvRequests.Columns.Add("Title", "Название");
            dgvRequests.Columns.Add("Device", "Устройство");
            dgvRequests.Columns.Add("Status", "Статус");
            dgvRequests.Columns.Add("Priority", "Приоритет");
            dgvRequests.Columns.Add("CreatedAt", "Создано");
            dgvRequests.Columns.Add("DaysOverdue", "Дней просрочки");

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
                var threshold = DateTime.UtcNow.AddDays(-(int)numDays.Value);
                var overdue = requests.Where(r => 
                    r.CreatedAt < threshold && 
                    (r.Status == RequestStatus.Pending || r.Status == RequestStatus.InProgress) &&
                    r.Status != RequestStatus.Completed
                ).OrderBy(r => r.CreatedAt);

                dgvRequests.Rows.Clear();
                foreach (var request in overdue)
                {
                    var days = (DateTime.UtcNow - request.CreatedAt).Days;
                    dgvRequests.Rows.Add(request.Id, request.Title, request.Device?.Name ?? "-", request.Status, request.Priority, request.CreatedAt.ToString("dd.MM.yyyy HH:mm"), days);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка заявок");
            }
        }
    }
}
