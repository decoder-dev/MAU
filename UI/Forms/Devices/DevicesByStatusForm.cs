using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Devices
{
    /// <summary>
    /// Устройства по статусу
    /// </summary>
    public class DevicesByStatusForm : Form
    {
        private DataGridView dgvDevices;
        private ComboBox cmbStatus;
        private Button btnRefresh;
        private GroupService? _groupService;

        public DevicesByStatusForm()
        {
            InitializeComponent();
            _groupService = new GroupService(Program.DbContext);
            LoadStatuses();
        }

        private void InitializeComponent()
        {
            this.Text = "Устройства по статусу";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilter = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lblStatus = new Label { Text = "Статус:", Location = new Point(15, 15), AutoSize = true };
            cmbStatus = new ComboBox { Location = new Point(70, 12), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.SelectedIndexChanged += (s, e) => LoadData();
            btnRefresh = new Button { Text = "Обновить", Location = new Point(280, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            panelFilter.Controls.AddRange(new Control[] { lblStatus, cmbStatus, btnRefresh });

            dgvDevices = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvDevices.Columns.Add("Name", "Название");
            dgvDevices.Columns.Add("IpAddress", "IP адрес");
            dgvDevices.Columns.Add("DeviceType", "Тип");
            dgvDevices.Columns.Add("Location", "Местоположение");
            dgvDevices.Columns.Add("LastSeen", "Последний контакт");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvDevices);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelFilter);
        }

        private void LoadStatuses()
        {
            cmbStatus.Items.Add("Все статусы");
            cmbStatus.Items.AddRange(Enum.GetNames(typeof(DeviceStatus)));
            cmbStatus.SelectedIndex = 0;
        }

        private async void LoadData()
        {
            try
            {
                var devicesByStatus = await _groupService!.GetDevicesByStatusAsync();
                dgvDevices.Rows.Clear();
                
                if (cmbStatus.SelectedIndex == 0)
                {
                    foreach (var group in devicesByStatus.OrderBy(g => g.Key))
                    {
                        foreach (var device in group.Value.OrderBy(d => d.Name))
                            dgvDevices.Rows.Add(device.Name, device.IpAddress, device.DeviceType.ToString(), device.Location ?? "-", device.LastSeen?.ToString("dd.MM.yyyy HH:mm") ?? "-");
                    }
                }
                else
                {
                    var selectedStatus = (DeviceStatus)Enum.Parse(typeof(DeviceStatus), cmbStatus.Text);
                    if (devicesByStatus.ContainsKey(selectedStatus))
                    {
                        foreach (var device in devicesByStatus[selectedStatus].OrderBy(d => d.Name))
                            dgvDevices.Rows.Add(device.Name, device.IpAddress, device.DeviceType.ToString(), device.Location ?? "-", device.LastSeen?.ToString("dd.MM.yyyy HH:mm") ?? "-");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка устройств");
            }
        }
    }
}
