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
    /// Редактирование заявки
    /// </summary>
    public partial class EditRequestForm : Form
    {
        private ComboBox cmbRequest;
        private Button btnEdit;

        public EditRequestForm()
        {
            InitializeComponent();
            LoadRequests();
        }

        private void InitializeComponent()
        {
            this.Text = "Редактирование заявки";
            this.Size = new Size(600, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            var lbl = new Label { Text = "Выберите заявку:", Location = new Point(20, 20), AutoSize = true };
            cmbRequest = new ComboBox { Location = new Point(20, 50), Width = 400, DropDownStyle = ComboBoxStyle.DropDownList };
            btnEdit = new Button { Text = "Редактировать", Location = new Point(430, 48), Width = 120 };
            btnEdit.Click += BtnEdit_Click;
            panel.Controls.AddRange(new Control[] { lbl, cmbRequest, btnEdit });
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

        private async void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (cmbRequest.SelectedItem == null)
            {
                ErrorHandler.ShowWarning("Выберите заявку");
                return;
            }
            
            var selected = cmbRequest.SelectedItem.GetType().GetProperty("Request")?.GetValue(cmbRequest.SelectedItem) as MaintenanceRequest;
            if (selected != null && selected.Device != null)
            {
                var form = new MaintenanceRequestForm(selected.Device, selected);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }
    }
}
