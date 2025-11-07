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
    /// Изменение статуса заявки
    /// </summary>
    public partial class ChangeRequestStatusForm : Form
    {
        private ComboBox cmbRequest;
        private ComboBox cmbStatus;
        private Button btnChange;

        public ChangeRequestStatusForm()
        {
            InitializeComponent();
            LoadRequests();
        }

        private void InitializeComponent()
        {
            this.Text = "Изменение статуса заявки";
            this.Size = new Size(600, 180);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            var lblRequest = new Label { Text = "Заявка:", Location = new Point(20, 20), AutoSize = true };
            cmbRequest = new ComboBox { Location = new Point(80, 17), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            var lblStatus = new Label { Text = "Статус:", Location = new Point(20, 60), AutoSize = true };
            cmbStatus = new ComboBox { Location = new Point(80, 57), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(Enum.GetNames(typeof(RequestStatus)));
            btnChange = new Button { Text = "Изменить", Location = new Point(290, 55), Width = 120 };
            btnChange.Click += BtnChange_Click;
            panel.Controls.AddRange(new Control[] { lblRequest, cmbRequest, lblStatus, cmbStatus, btnChange });
            this.Controls.Add(panel);
        }

        private async void LoadRequests()
        {
            try
            {
                var requests = await DataService.GetMaintenanceRequestsAsync();
                cmbRequest.Items.Clear();
                foreach (var request in requests.OrderByDescending(r => r.CreatedAt))
                    cmbRequest.Items.Add(new { Request = request, Display = $"[{request.Id}] {request.Title} ({request.Status})" });
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

        private async void BtnChange_Click(object? sender, EventArgs e)
        {
            if (cmbRequest.SelectedItem == null)
            {
                ErrorHandler.ShowWarning("Выберите заявку");
                return;
            }

            var request = cmbRequest.SelectedItem.GetType().GetProperty("Request")?.GetValue(cmbRequest.SelectedItem) as MaintenanceRequest;
            if (request != null)
            {
                request.Status = (RequestStatus)Enum.Parse(typeof(RequestStatus), cmbStatus.Text);
                if (request.Status == RequestStatus.Completed)
                    request.CompletedAt = DateTime.UtcNow;
                
                var success = await DataService.UpdateMaintenanceRequestAsync(request);
                if (success)
                {
                    ErrorHandler.ShowSuccess("Статус изменен");
                    LoadRequests();
                }
                else
                    ErrorHandler.ShowError("Не удалось изменить статус");
            }
        }
    }
}
