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
    /// Назначение исполнителя
    /// </summary>
    public partial class AssignRequestForm : Form
    {
        private ComboBox cmbRequest;
        private ComboBox cmbAssignee;
        private Button btnAssign;
        private Label lblInfo;

        public AssignRequestForm()
        {
            InitializeComponent();
            LoadRequests();
            LoadAssignees();
        }

        private void InitializeComponent()
        {
            this.Text = "Назначение исполнителя";
            this.Size = new Size(600, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            var lblRequest = new Label { Text = "Заявка:", Location = new Point(20, 20), AutoSize = true };
            cmbRequest = new ComboBox { Location = new Point(80, 17), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            var lblAssignee = new Label { Text = "Исполнитель:", Location = new Point(20, 60), AutoSize = true };
            cmbAssignee = new ComboBox { Location = new Point(100, 57), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            btnAssign = new Button { Text = "Назначить", Location = new Point(410, 35), Width = 120 };
            btnAssign.Click += BtnAssign_Click;
            lblInfo = new Label { Location = new Point(20, 100), Width = 500 };
            panel.Controls.AddRange(new Control[] { lblRequest, cmbRequest, lblAssignee, cmbAssignee, btnAssign, lblInfo });
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

        private async void LoadAssignees()
        {
            try
            {
                var users = await DataService.GetUsersAsync();
                cmbAssignee.Items.Clear();
                foreach (var user in users.Where(u => u.IsActive).OrderBy(u => u.Username))
                    cmbAssignee.Items.Add(new { User = user, Display = $"{user.Username} ({user.FullName ?? "-"})" });
                if (cmbAssignee.Items.Count > 0)
                    cmbAssignee.DisplayMember = "Display";
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка пользователей");
            }
        }

        private async void BtnAssign_Click(object? sender, EventArgs e)
        {
            if (cmbRequest.SelectedItem == null || cmbAssignee.SelectedItem == null)
            {
                ErrorHandler.ShowWarning("Выберите заявку и исполнителя");
                return;
            }

            var request = cmbRequest.SelectedItem.GetType().GetProperty("Request")?.GetValue(cmbRequest.SelectedItem) as MaintenanceRequest;
            var user = cmbAssignee.SelectedItem.GetType().GetProperty("User")?.GetValue(cmbAssignee.SelectedItem) as User;
            
            if (request != null && user != null)
            {
                request.AssignedTo = user.Username;
                var success = await DataService.UpdateMaintenanceRequestAsync(request);
                if (success)
                {
                    lblInfo.Text = $"Исполнитель {user.Username} назначен на заявку {request.Id}";
                    ErrorHandler.ShowSuccess("Исполнитель назначен");
                }
                else
                    ErrorHandler.ShowError("Не удалось назначить исполнителя");
            }
        }
    }
}
