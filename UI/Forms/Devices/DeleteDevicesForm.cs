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
    /// Удаление устройств
    /// </summary>
    public class DeleteDevicesForm : Form
    {
        private ComboBox cmbDevice = null!;
        private Button btnDelete = null!;
        private TextBox txtInfo = null!;

        public DeleteDevicesForm()
        {
            InitializeComponent();
            LoadDevices();
        }

        private void InitializeComponent()
        {
            this.Text = "Удаление устройства";
            this.Size = new Size(600, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            var lbl = new Label { Text = "Выберите устройство для удаления:", Location = new Point(20, 20), AutoSize = true };
            cmbDevice = new ComboBox { Location = new Point(20, 50), Width = 400, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDevice.SelectedIndexChanged += (s, e) => UpdateInfo();
            txtInfo = new TextBox { Location = new Point(20, 85), Width = 400, Height = 60, Multiline = true, ReadOnly = true };
            btnDelete = new Button { Text = "Удалить", Location = new Point(430, 48), Width = 120, BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White };
            btnDelete.Click += BtnDelete_Click;
            panel.Controls.AddRange(new Control[] { lbl, cmbDevice, txtInfo, btnDelete });
            this.Controls.Add(panel);
        }

        private async void LoadDevices()
        {
            try
            {
                var devices = await DataService.GetDevicesAsync();
                cmbDevice.Items.Clear();
                foreach (var device in devices.OrderBy(d => d.Name))
                    cmbDevice.Items.Add(new { Device = device, Display = $"{device.Name} ({device.IpAddress})" });
                if (cmbDevice.Items.Count > 0)
                    cmbDevice.DisplayMember = "Display";
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка устройств");
            }
        }

        private void UpdateInfo()
        {
            if (cmbDevice.SelectedItem == null) return;
            var selected = cmbDevice.SelectedItem.GetType().GetProperty("Device")?.GetValue(cmbDevice.SelectedItem) as NetworkDevice;
            if (selected != null)
                txtInfo.Text = $"Устройство: {selected.Name}\nIP: {selected.IpAddress}\nТип: {selected.DeviceType}\nСтатус: {selected.Status}";
        }

        private async void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (cmbDevice.SelectedItem == null)
            {
                ErrorHandler.ShowWarning("Выберите устройство");
                return;
            }
            if (!ErrorHandler.ShowConfirm("Удалить выбранное устройство?", "Подтверждение"))
                return;

            var selected = cmbDevice.SelectedItem.GetType().GetProperty("Device")?.GetValue(cmbDevice.SelectedItem) as NetworkDevice;
            if (selected != null)
            {
                var success = await DataService.DeleteDeviceAsync(selected);
                if (success)
                {
                    ErrorHandler.ShowSuccess("Устройство удалено");
                    LoadDevices();
                }
                else
                    ErrorHandler.ShowError("Не удалось удалить устройство");
            }
        }
    }
}