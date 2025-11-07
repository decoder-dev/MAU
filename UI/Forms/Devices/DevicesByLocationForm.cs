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
    /// Устройства по местоположению
    /// </summary>
    public class DevicesByLocationForm : Form
    {
        private DataGridView dgvDevices;
        private ComboBox cmbLocation;
        private Button btnRefresh;
        private GroupService? _groupService;

        public DevicesByLocationForm()
        {
            InitializeComponent();
            _groupService = new GroupService(Program.DbContext);
            LoadLocations();
        }

        private void InitializeComponent()
        {
            this.Text = "Устройства по местоположению";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilter = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lblLocation = new Label { Text = "Местоположение:", Location = new Point(15, 15), AutoSize = true };
            cmbLocation = new ComboBox { Location = new Point(120, 12), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbLocation.SelectedIndexChanged += (s, e) => LoadData();
            btnRefresh = new Button { Text = "Обновить", Location = new Point(430, 10), Width = 100 };
            btnRefresh.Click += (s, e) => { LoadLocations(); LoadData(); };
            panelFilter.Controls.AddRange(new Control[] { lblLocation, cmbLocation, btnRefresh });

            dgvDevices = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvDevices.Columns.Add("Name", "Название");
            dgvDevices.Columns.Add("IpAddress", "IP адрес");
            dgvDevices.Columns.Add("DeviceType", "Тип");
            dgvDevices.Columns.Add("Status", "Статус");
            dgvDevices.Columns.Add("Building", "Здание");
            dgvDevices.Columns.Add("Room", "Помещение");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvDevices);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelFilter);
        }

        private async void LoadLocations()
        {
            try
            {
                var devicesByLocation = await _groupService!.GetDevicesByLocationAsync();
                cmbLocation.Items.Clear();
                cmbLocation.Items.Add("Все местоположения");
                foreach (var location in devicesByLocation.Keys.OrderBy(l => l))
                    cmbLocation.Items.Add(location);
                if (cmbLocation.Items.Count > 0)
                    cmbLocation.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка местоположений");
            }
        }

        private async void LoadData()
        {
            try
            {
                var devicesByLocation = await _groupService!.GetDevicesByLocationAsync();
                dgvDevices.Rows.Clear();
                
                if (cmbLocation.SelectedIndex == 0)
                {
                    foreach (var group in devicesByLocation.OrderBy(g => g.Key))
                    {
                        foreach (var device in group.Value.OrderBy(d => d.Name))
                            dgvDevices.Rows.Add(device.Name, device.IpAddress, device.DeviceType.ToString(), device.Status.ToString(), device.Building ?? "-", device.Room ?? "-");
                    }
                }
                else
                {
                    var selectedLocation = cmbLocation.Text;
                    if (devicesByLocation.ContainsKey(selectedLocation))
                    {
                        foreach (var device in devicesByLocation[selectedLocation].OrderBy(d => d.Name))
                            dgvDevices.Rows.Add(device.Name, device.IpAddress, device.DeviceType.ToString(), device.Status.ToString(), device.Building ?? "-", device.Room ?? "-");
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
