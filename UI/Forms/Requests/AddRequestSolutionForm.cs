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
    /// Добавление решения
    /// </summary>
    public partial class AddRequestSolutionForm : Form
    {
        private ComboBox cmbRequest;
        private TextBox txtResolution;
        private Button btnSave;

        public AddRequestSolutionForm()
        {
            InitializeComponent();
            LoadRequests();
        }

        private void InitializeComponent()
        {
            this.Text = "Добавление решения";
            this.Size = new Size(700, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            var lblRequest = new Label { Text = "Заявка:", Location = new Point(20, 20), AutoSize = true };
            cmbRequest = new ComboBox { Location = new Point(80, 17), Width = 400, DropDownStyle = ComboBoxStyle.DropDownList };
            var lblResolution = new Label { Text = "Решение:", Location = new Point(20, 60), AutoSize = true };
            txtResolution = new TextBox { Location = new Point(20, 85), Width = 500, Height = 100, Multiline = true };
            btnSave = new Button { Text = "Сохранить", Location = new Point(540, 145), Width = 100 };
            btnSave.Click += BtnSave_Click;
            panel.Controls.AddRange(new Control[] { lblRequest, cmbRequest, lblResolution, txtResolution, btnSave });
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

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (cmbRequest.SelectedItem == null || string.IsNullOrWhiteSpace(txtResolution.Text))
            {
                ErrorHandler.ShowWarning("Выберите заявку и введите решение");
                return;
            }

            var request = cmbRequest.SelectedItem.GetType().GetProperty("Request")?.GetValue(cmbRequest.SelectedItem) as MaintenanceRequest;
            if (request != null)
            {
                request.Resolution = txtResolution.Text;
                var success = await DataService.UpdateMaintenanceRequestAsync(request);
                if (success)
                {
                    ErrorHandler.ShowSuccess("Решение добавлено");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                    ErrorHandler.ShowError("Не удалось сохранить решение");
            }
        }
    }
}
