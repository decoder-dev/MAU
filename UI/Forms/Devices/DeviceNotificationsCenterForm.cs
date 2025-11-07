using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Devices
{
    /// <summary>
    /// Уведомления
    /// </summary>
    public partial class DeviceNotificationsCenterForm : Form
    {
        private DataGridView dgvNotifications;
        private ComboBox cmbType;
        private Button btnRefresh;
        private Button btnClear;
        private NotificationService? _notificationService;

        public DeviceNotificationsCenterForm()
        {
            InitializeComponent();
            _notificationService = new NotificationService(Program.DbContext);
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Уведомления";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilters = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lblType = new Label { Text = "Тип:", Location = new Point(15, 15), AutoSize = true };
            cmbType = new ComboBox { Location = new Point(50, 12), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbType.Items.AddRange(new[] { "Все", "Email", "SMS", "Системные" });
            cmbType.SelectedIndex = 0;
            cmbType.SelectedIndexChanged += (s, e) => LoadData();
            btnRefresh = new Button { Text = "Обновить", Location = new Point(260, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            btnClear = new Button { Text = "Очистить", Location = new Point(370, 10), Width = 100, BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White };
            btnClear.Click += BtnClear_Click;
            panelFilters.Controls.AddRange(new Control[] { lblType, cmbType, btnRefresh, btnClear });

            dgvNotifications = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvNotifications.Columns.Add("Device", "Устройство");
            dgvNotifications.Columns.Add("Type", "Тип");
            dgvNotifications.Columns.Add("Message", "Сообщение");
            dgvNotifications.Columns.Add("Timestamp", "Время");
            dgvNotifications.Columns.Add("Status", "Статус");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvNotifications);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelFilters);
        }

        private async void LoadData()
        {
            try
            {
                dgvNotifications.Rows.Clear();
                var requests = await DataService.GetMaintenanceRequestsAsync();
                var devices = await DataService.GetDevicesAsync();
                var statusHistory = await Program.DbContext.DeviceStatusHistory
                    .OrderByDescending(h => h.Timestamp)
                    .Take(100)
                    .ToListAsync();
                
                var filterType = cmbType.SelectedItem?.ToString();
                
                foreach (var history in statusHistory)
                {
                    if (filterType != "Все" && filterType != "Системные") continue;
                    
                    var device = devices.FirstOrDefault(d => d.Id == history.DeviceId);
                    var message = $"Устройство {device?.Name ?? "Неизвестно"} изменило статус на {history.Status}";
                    dgvNotifications.Rows.Add(
                        device?.Name ?? "Неизвестно",
                        "Системное",
                        message,
                        history.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                        "Отправлено"
                    );
                }
                
                foreach (var request in requests.OrderByDescending(r => r.CreatedAt).Take(50))
                {
                    if (filterType != "Все" && filterType != "Email") continue;
                    
                    var device = devices.FirstOrDefault(d => d.Id == request.DeviceId);
                    dgvNotifications.Rows.Add(
                        device?.Name ?? "Неизвестно",
                        "Email",
                        $"Заявка: {request.Title}",
                        request.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        request.Status.ToString()
                    );
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка уведомлений");
            }
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            if (ErrorHandler.ShowConfirm("Очистить все уведомления?", "Подтверждение"))
            {
                dgvNotifications.Rows.Clear();
                ErrorHandler.ShowSuccess("Уведомления очищены");
            }
        }
    }
}
