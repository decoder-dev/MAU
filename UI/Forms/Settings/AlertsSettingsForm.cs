using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки алертов
    /// </summary>
    public partial class AlertsSettingsForm : Form
    {
        private DataGridView dgvAlerts;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;

        public AlertsSettingsForm()
        {
            InitializeComponent();
            LoadAlerts();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки алертов";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnAdd = new Button { Text = "Добавить", Location = new Point(15, 10), Width = 100 };
            btnEdit = new Button { Text = "Редактировать", Location = new Point(125, 10), Width = 100 };
            btnDelete = new Button { Text = "Удалить", Location = new Point(235, 10), Width = 100 };
            panelTop.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });

            dgvAlerts = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvAlerts.Columns.Add("Name", "Название");
            dgvAlerts.Columns.Add("Condition", "Условие");
            dgvAlerts.Columns.Add("Action", "Действие");
            dgvAlerts.Columns.Add("Enabled", "Включено");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvAlerts);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private async void LoadAlerts()
        {
            try
            {
                dgvAlerts.Rows.Clear();
                var devices = await DataService.GetDevicesAsync();
                var statusHistory = await Program.DbContext.DeviceStatusHistory
                    .Where(h => h.Status == DeviceStatus.Offline || h.Status == DeviceStatus.Degraded)
                    .OrderByDescending(h => h.Timestamp)
                    .Take(50)
                    .ToListAsync();
                foreach (var history in statusHistory)
                {
                    var device = devices.FirstOrDefault(d => d.Id == history.DeviceId);
                    dgvAlerts.Rows.Add(device?.Name ?? "Неизвестно", history.Status.ToString(), history.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка алертов");
            }
        }
    }
}
