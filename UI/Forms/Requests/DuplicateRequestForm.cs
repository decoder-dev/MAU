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
    /// Дублирование заявки
    /// </summary>
    public partial class DuplicateRequestForm : Form
    {
        private ComboBox cmbRequest;
        private Button btnDuplicate;

        public DuplicateRequestForm()
        {
            InitializeComponent();
            LoadRequests();
        }

        private void InitializeComponent()
        {
            this.Text = "Дублирование заявки";
            this.Size = new Size(600, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            var lbl = new Label { Text = "Выберите заявку для дублирования:", Location = new Point(20, 20), AutoSize = true };
            cmbRequest = new ComboBox { Location = new Point(20, 50), Width = 400, DropDownStyle = ComboBoxStyle.DropDownList };
            btnDuplicate = new Button { Text = "Дублировать", Location = new Point(430, 48), Width = 120 };
            btnDuplicate.Click += BtnDuplicate_Click;
            panel.Controls.AddRange(new Control[] { lbl, cmbRequest, btnDuplicate });
            this.Controls.Add(panel);
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
                    cmbRequest.DisplayMember = "Display";
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка заявок");
            }
        }

        private async void BtnDuplicate_Click(object? sender, EventArgs e)
        {
            if (cmbRequest.SelectedItem == null)
            {
                ErrorHandler.ShowWarning("Выберите заявку");
                return;
            }

            var original = cmbRequest.SelectedItem.GetType().GetProperty("Request")?.GetValue(cmbRequest.SelectedItem) as MaintenanceRequest;
            if (original != null)
            {
                var duplicate = new MaintenanceRequest
                {
                    Title = $"{original.Title} (копия)",
                    Description = original.Description,
                    DeviceId = original.DeviceId,
                    UserId = original.UserId,
                    Status = RequestStatus.Pending,
                    Priority = original.Priority,
                    CreatedAt = DateTime.UtcNow
                };
                
                var success = await DataService.AddMaintenanceRequestAsync(duplicate);
                if (success)
                {
                    ErrorHandler.ShowSuccess("Заявка продублирована");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                    ErrorHandler.ShowError("Не удалось продублировать заявку");
            }
        }
    }
}