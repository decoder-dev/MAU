using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;

namespace NetworkMonitoringSystem.UI.Forms.Devices
{
    /// <summary>
    /// Добавление устройства
    /// </summary>
    public class AddDeviceForm : Form
    {
        public AddDeviceForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Добавление устройства";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var btnOpen = new Button
            {
                Text = "Открыть форму добавления устройства",
                Dock = DockStyle.Fill,
                Margin = new Padding(20)
            };
            btnOpen.Click += (s, e) =>
            {
                var deviceForm = new DeviceForm();
                if (deviceForm.ShowDialog() == DialogResult.OK)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            };

            this.Controls.Add(btnOpen);
        }
    }
}