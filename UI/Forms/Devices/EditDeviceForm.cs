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
    /// Редактирование устройства
    /// </summary>
    public class EditDeviceForm : Form
    {
        private ComboBox cmbDevice = null!;
        private Button btnEdit = null!;
        
        public EditDeviceForm()
        {
            InitializeComponent();
            LoadDevices();
        }

        private void InitializeComponent()
        {
            this.Text = "Редактирование устройства";
            this.Size = new Size(600, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            
            var lbl = new Label { Text = "Выберите устройство:", Location = new Point(20, 20), AutoSize = true };
            cmbDevice = new ComboBox
            {
                Location = new Point(20, 50),
                Width = 400,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            
            btnEdit = new Button
            {
                Text = "Редактировать",
                Location = new Point(430, 48),
                Width = 120
            };
            btnEdit.Click += BtnEdit_Click;
            
            panel.Controls.AddRange(new Control[] { lbl, cmbDevice, btnEdit });
            this.Controls.Add(panel);
        }

        private async void LoadDevices()
        {
            try
            {
                var devices = await DataService.GetDevicesAsync();
                cmbDevice.Items.Clear();
                foreach (var device in devices.OrderBy(d => d.Name))
                {
                    cmbDevice.Items.Add(new { Device = device, Display = $"{device.Name} ({device.IpAddress})" });
                }
                
                if (cmbDevice.Items.Count > 0)
                    cmbDevice.DisplayMember = "Display";
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка устройств");
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (cmbDevice.SelectedItem == null)
            {
                ErrorHandler.ShowWarning("Выберите устройство");
                return;
            }
            
            var selected = cmbDevice.SelectedItem.GetType().GetProperty("Device")?.GetValue(cmbDevice.SelectedItem) as NetworkDevice;
            if (selected != null)
            {
                var form = new DeviceForm(selected);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }
    }
}