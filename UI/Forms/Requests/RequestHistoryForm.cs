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
    /// Просмотр истории заявки
    /// </summary>
    public partial class RequestHistoryForm : Form
    {
        private DataGridView dgvHistory;
        private ComboBox cmbRequest;

        public RequestHistoryForm()
        {
            InitializeComponent();
            LoadRequests();
        }

        private void InitializeComponent()
        {
            this.Text = "Просмотр истории заявки";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lbl = new Label { Text = "Заявка:", Location = new Point(15, 15), AutoSize = true };
            cmbRequest = new ComboBox { Location = new Point(75, 12), Width = 400, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRequest.SelectedIndexChanged += (s, e) => LoadHistory();
            panelTop.Controls.AddRange(new Control[] { lbl, cmbRequest });

            dgvHistory = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvHistory.Columns.Add("Date", "Дата");
            dgvHistory.Columns.Add("Action", "Действие");
            dgvHistory.Columns.Add("User", "Пользователь");
            dgvHistory.Columns.Add("Details", "Детали");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvHistory);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private async void LoadRequests()
        {
            try
            {
                var requests = await DataService.GetMaintenanceRequestsAsync();
                cmbRequest.Items.Clear();
                foreach (var request in requests.OrderByDescending(r => r.CreatedAt))
                    cmbRequest.Items.Add(new { Request = request, Display = $"[{request.Id}] {request.Title}" });
                if (cmbRequest.Items.Count > 0)
                {
                    cmbRequest.DisplayMember = "Display";
                    cmbRequest.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка заявок");
            }
        }

        private void LoadHistory()
        {
            try
            {
                var request = cmbRequest.SelectedItem?.GetType().GetProperty("Request")?.GetValue(cmbRequest.SelectedItem) as MaintenanceRequest;
                if (request == null) return;

                dgvHistory.Rows.Clear();
                dgvHistory.Rows.Add(request.CreatedAt.ToString("dd.MM.yyyy HH:mm"), "Создана", request.User?.Username ?? "-", $"Приоритет: {request.Priority}");
                if (request.UpdatedAt.HasValue)
                    dgvHistory.Rows.Add(request.UpdatedAt.Value.ToString("dd.MM.yyyy HH:mm"), "Обновлена", "-", $"Статус: {request.Status}");
                if (request.CompletedAt.HasValue)
                    dgvHistory.Rows.Add(request.CompletedAt.Value.ToString("dd.MM.yyyy HH:mm"), "Завершена", request.AssignedTo ?? "-", request.Resolution ?? "-");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка истории");
            }
        }
    }
}
