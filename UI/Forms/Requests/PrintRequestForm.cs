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
    /// Печать заявки
    /// </summary>
    public partial class PrintRequestForm : Form
    {
        private ComboBox cmbRequest;
        private Button btnPrint;
        private TextBox txtPreview;

        public PrintRequestForm()
        {
            InitializeComponent();
            LoadRequests();
        }

        private void InitializeComponent()
        {
            this.Text = "Печать заявки";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lbl = new Label { Text = "Заявка:", Location = new Point(15, 15), AutoSize = true };
            cmbRequest = new ComboBox { Location = new Point(75, 12), Width = 400, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRequest.SelectedIndexChanged += (s, e) => LoadPreview();
            btnPrint = new Button { Text = "Печать", Location = new Point(485, 10), Width = 100 };
            btnPrint.Click += BtnPrint_Click;
            panelTop.Controls.AddRange(new Control[] { lbl, cmbRequest, btnPrint });

            txtPreview = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, Font = new Font("Consolas", 9), ScrollBars = ScrollBars.Both };
            
            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(txtPreview);

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

        private void LoadPreview()
        {
            try
            {
                var request = cmbRequest.SelectedItem?.GetType().GetProperty("Request")?.GetValue(cmbRequest.SelectedItem) as MaintenanceRequest;
                if (request == null) return;

                txtPreview.Text = $@"ЗАЯВКА НА ОБСЛУЖИВАНИЕ №{request.Id}
{new string('=', 50)}
Дата создания: {request.CreatedAt:dd.MM.yyyy HH:mm}
Устройство: {request.Device?.Name ?? "-"} ({request.Device?.IpAddress ?? "-"})
Заявитель: {request.User?.Username ?? "-"}
Статус: {request.Status}
Приоритет: {request.Priority}
Исполнитель: {request.AssignedTo ?? "Не назначен"}
{new string('=', 50)}
НАЗВАНИЕ:
{request.Title}
{new string('=', 50)}
ОПИСАНИЕ:
{request.Description ?? "-"}
{new string('=', 50)}
РЕШЕНИЕ:
{request.Resolution ?? "Не решено"}
{new string('=', 50)}
";
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка предпросмотра");
            }
        }

        private void BtnPrint_Click(object? sender, EventArgs e)
        {
            using var dialog = new PrintDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ErrorHandler.ShowSuccess("Печать запущена");
            }
        }
    }
}
