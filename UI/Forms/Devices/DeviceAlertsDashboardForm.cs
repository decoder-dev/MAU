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
    /// Алерты устройств
    /// </summary>
    public partial class DeviceAlertsDashboardForm : Form
    {
        private DataGridView dgvAlerts;
        private ComboBox cmbAlertType;
        private Button btnRefresh;
        private Button btnClear;
        private Button btnExport;

        public DeviceAlertsDashboardForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Алерты устройств";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilters = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lblType = new Label { Text = "Тип алерта:", Location = new Point(15, 15), AutoSize = true };
            cmbAlertType = new ComboBox { Location = new Point(90, 12), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbAlertType.Items.AddRange(new[] { "Все", "Критический", "Предупреждение", "Информация" });
            cmbAlertType.SelectedIndex = 0;
            cmbAlertType.SelectedIndexChanged += (s, e) => LoadData();
            btnRefresh = new Button { Text = "Обновить", Location = new Point(300, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            btnClear = new Button { Text = "Очистить", Location = new Point(410, 10), Width = 100, BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White };
            btnClear.Click += BtnClear_Click;
            btnExport = new Button { Text = "Экспорт", Location = new Point(520, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelFilters.Controls.AddRange(new Control[] { lblType, cmbAlertType, btnRefresh, btnClear, btnExport });

            dgvAlerts = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvAlerts.Columns.Add("Device", "Устройство");
            dgvAlerts.Columns.Add("IpAddress", "IP адрес");
            dgvAlerts.Columns.Add("AlertType", "Тип");
            dgvAlerts.Columns.Add("Message", "Сообщение");
            dgvAlerts.Columns.Add("Timestamp", "Время");
            dgvAlerts.Columns.Add("Status", "Статус");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvAlerts);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelFilters);
        }

        private async void LoadData()
        {
            try
            {
                var devices = await DataService.GetDevicesAsync();
                dgvAlerts.Rows.Clear();
                
                // Генерируем алерты на основе статусов устройств
                foreach (var device in devices.OrderByDescending(d => d.UpdatedAt))
                {
                    if (device.Status == DeviceStatus.Offline)
                    {
                        if (cmbAlertType.SelectedIndex == 0 || cmbAlertType.Text == "Критический")
                        {
                            dgvAlerts.Rows.Add(
                                device.Name,
                                device.IpAddress,
                                "Критический",
                                $"Устройство {device.Name} недоступно",
                                device.UpdatedAt.ToString("dd.MM.yyyy HH:mm"),
                                "Активен"
                            );
                        }
                    }
                    else if (device.Status == DeviceStatus.Degraded)
                    {
                        if (cmbAlertType.SelectedIndex == 0 || cmbAlertType.Text == "Предупреждение")
                        {
                            dgvAlerts.Rows.Add(
                                device.Name,
                                device.IpAddress,
                                "Предупреждение",
                                $"Устройство {device.Name} работает некорректно",
                                device.UpdatedAt.ToString("dd.MM.yyyy HH:mm"),
                                "Активен"
                            );
                        }
                    }
                    else if (device.Status == DeviceStatus.Online && device.LastSeen.HasValue && (DateTime.UtcNow - device.LastSeen.Value).TotalDays > 7)
                    {
                        if (cmbAlertType.SelectedIndex == 0 || cmbAlertType.Text == "Информация")
                        {
                            dgvAlerts.Rows.Add(
                                device.Name,
                                device.IpAddress,
                                "Информация",
                                $"Устройство {device.Name} не обновлялось более 7 дней",
                                device.UpdatedAt.ToString("dd.MM.yyyy HH:mm"),
                                "Активен"
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка алертов");
            }
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            if (ErrorHandler.ShowConfirm("Очистить все алерты?", "Подтверждение"))
            {
                dgvAlerts.Rows.Clear();
                ErrorHandler.ShowSuccess("Алерты очищены");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"alerts_{DateTime.Now:yyyyMMdd}.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Устройство;IP адрес;Тип;Сообщение;Время;Статус");
                foreach (DataGridViewRow row in dgvAlerts.Rows)
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value};{row.Cells[3].Value};{row.Cells[4].Value};{row.Cells[5].Value}");
                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Данные экспортированы");
            }
        }
    }
}
