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
    /// Удаление заявки
    /// </summary>
    public partial class DeleteRequestForm : Form
    {
        private ComboBox cmbRequest;
        private Button btnDelete;

        public DeleteRequestForm()
        {
            InitializeComponent();
            LoadRequests();
        }

        private void InitializeComponent()
        {
            this.Text = "Удаление заявки";
            this.Size = new Size(600, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            var lbl = new Label { Text = "Выберите заявку для удаления:", Location = new Point(20, 20), AutoSize = true };
            cmbRequest = new ComboBox { Location = new Point(20, 50), Width = 400, DropDownStyle = ComboBoxStyle.DropDownList };
            btnDelete = new Button { Text = "Удалить", Location = new Point(430, 48), Width = 120, BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White };
            btnDelete.Click += BtnDelete_Click;
            panel.Controls.AddRange(new Control[] { lbl, cmbRequest, btnDelete });
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

        private async void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (cmbRequest.SelectedItem == null)
            {
                ErrorHandler.ShowWarning("Выберите заявку");
                return;
            }
            if (!ErrorHandler.ShowConfirm("Удалить выбранную заявку?", "Подтверждение"))
                return;

            var selected = cmbRequest.SelectedItem.GetType().GetProperty("Request")?.GetValue(cmbRequest.SelectedItem) as MaintenanceRequest;
            if (selected != null)
            {
                var success = await DataService.DeleteMaintenanceRequestAsync(selected);
                if (success)
                {
                    ErrorHandler.ShowSuccess("Заявка удалена");
                    LoadRequests();
                }
                else
                    ErrorHandler.ShowError("Не удалось удалить заявку");
            }
        }
    }
}
