using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Devices
{
    /// <summary>
    /// Метрики устройств
    /// </summary>
    public partial class DeviceMetricsOverviewForm : Form
    {
        private DataGridView dgvMetrics;
        private ComboBox cmbDevice;
        private Button btnRefresh;
        private Button btnExport;

        public DeviceMetricsOverviewForm()
        {
            InitializeComponent();
            LoadDevices();
        }

        private void InitializeComponent()
        {
            this.Text = "Метрики устройств";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilters = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lblDevice = new Label { Text = "Устройство:", Location = new Point(15, 15), AutoSize = true };
            cmbDevice = new ComboBox { Location = new Point(85, 12), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDevice.SelectedIndexChanged += (s, e) => LoadData();
            btnRefresh = new Button { Text = "Обновить", Location = new Point(400, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            btnExport = new Button { Text = "Экспорт", Location = new Point(510, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelFilters.Controls.AddRange(new Control[] { lblDevice, cmbDevice, btnRefresh, btnExport });

            dgvMetrics = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvMetrics.Columns.Add("Metric", "Метрика");
            dgvMetrics.Columns.Add("Value", "Значение");
            dgvMetrics.Columns.Add("Unit", "Единица измерения");
            dgvMetrics.Columns.Add("LastUpdate", "Последнее обновление");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvMetrics);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelFilters);
        }

        private async void LoadDevices()
        {
            try
            {
                var devices = await DataService.GetDevicesAsync();
                cmbDevice.Items.Clear();
                cmbDevice.Items.Add("Все устройства");
                foreach (var device in devices.OrderBy(d => d.Name))
                    cmbDevice.Items.Add(new { Device = device, Display = $"{device.Name} ({device.IpAddress})" });
                if (cmbDevice.Items.Count > 0)
                {
                    cmbDevice.DisplayMember = "Display";
                    cmbDevice.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка устройств");
            }
        }

        private async void LoadData()
        {
            try
            {
                dgvMetrics.Rows.Clear();
                
                if (cmbDevice.SelectedIndex == 0)
                {
                    // Показать агрегированные метрики для всех устройств
                    var devices = await DataService.GetDevicesAsync();
                    var onlineCount = devices.Count(d => d.Status == DeviceStatus.Online);
                    var offlineCount = devices.Count(d => d.Status == DeviceStatus.Offline);
                    var totalCount = devices.Count;
                    
                    dgvMetrics.Rows.Add("Всего устройств", totalCount, "шт", DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                    dgvMetrics.Rows.Add("Онлайн", onlineCount, "шт", DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                    dgvMetrics.Rows.Add("Офлайн", offlineCount, "шт", DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                    dgvMetrics.Rows.Add("Процент доступности", totalCount > 0 ? $"{100.0 * onlineCount / totalCount:F1}" : "0", "%", DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                }
                else
                {
                    var selected = cmbDevice.SelectedItem?.GetType().GetProperty("Device")?.GetValue(cmbDevice.SelectedItem) as NetworkDevice;
                    if (selected != null)
                    {
                        dgvMetrics.Rows.Add("Название", selected.Name, "-", selected.UpdatedAt.ToString("dd.MM.yyyy HH:mm"));
                        dgvMetrics.Rows.Add("IP адрес", selected.IpAddress, "-", selected.UpdatedAt.ToString("dd.MM.yyyy HH:mm"));
                        dgvMetrics.Rows.Add("Статус", selected.Status.ToString(), "-", selected.UpdatedAt.ToString("dd.MM.yyyy HH:mm"));
                        dgvMetrics.Rows.Add("Тип", selected.DeviceType.ToString(), "-", selected.UpdatedAt.ToString("dd.MM.yyyy HH:mm"));
                        dgvMetrics.Rows.Add("Последний контакт", selected.LastSeen?.ToString("dd.MM.yyyy HH:mm") ?? "Никогда", "-", selected.UpdatedAt.ToString("dd.MM.yyyy HH:mm"));
                        dgvMetrics.Rows.Add("Местоположение", selected.Location ?? "-", "-", selected.UpdatedAt.ToString("dd.MM.yyyy HH:mm"));
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка метрик");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"device_metrics_{DateTime.Now:yyyyMMdd}.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Метрика;Значение;Единица измерения;Последнее обновление");
                foreach (DataGridViewRow row in dgvMetrics.Rows)
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value};{row.Cells[3].Value}");
                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Данные экспортированы");
            }
        }
    }
}
