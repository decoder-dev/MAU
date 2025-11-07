using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Requests
{
    /// <summary>
    /// Уведомления по заявкам
    /// </summary>
    public partial class RequestNotificationsForm : Form
    {
        private DataGridView dgvNotifications;
        private Button btnRefresh;
        private CheckBox chkEmail;
        private CheckBox chkSms;

        public RequestNotificationsForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Уведомления по заявкам";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            chkEmail = new CheckBox { Text = "Email", Location = new Point(15, 15), Checked = true };
            chkSms = new CheckBox { Text = "SMS", Location = new Point(100, 15) };
            btnRefresh = new Button { Text = "Обновить", Location = new Point(200, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            panelTop.Controls.AddRange(new Control[] { chkEmail, chkSms, btnRefresh });

            dgvNotifications = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvNotifications.Columns.Add("RequestId", "ID заявки");
            dgvNotifications.Columns.Add("Type", "Тип");
            dgvNotifications.Columns.Add("Recipient", "Получатель");
            dgvNotifications.Columns.Add("Sent", "Отправлено");
            dgvNotifications.Columns.Add("Status", "Статус");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvNotifications);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private async void LoadData()
        {
            try
            {
                dgvNotifications.Rows.Clear();
                var requests = await DataService.GetMaintenanceRequestsAsync();
                var devices = await DataService.GetDevicesAsync();
                var statusHistory = await Program.DbContext.DeviceStatusHistory.OrderByDescending(h => h.Timestamp).Take(100).ToListAsync();
                
                foreach (var history in statusHistory)
                {
                    var device = devices.FirstOrDefault(d => d.Id == history.DeviceId);
                    dgvNotifications.Rows.Add(device?.Name ?? "Неизвестно", $"Статус: {history.Status}", history.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка уведомлений");
            }
        }
    }
}
