using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Devices
{
    /// <summary>
    /// Сканирование сети
    /// </summary>
    public partial class DeviceScanWrapperForm : Form
    {
        public DeviceScanWrapperForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Сканирование сети";
            this.Size = new Size(600, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var btnOpen = new Button
            {
                Text = "Открыть форму сканирования сети",
                Dock = DockStyle.Fill,
                Margin = new Padding(20)
            };
            btnOpen.Click += (s, e) =>
            {
                var form = new DeviceScanForm();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            };
            
            this.Controls.Add(btnOpen);
        }
    }
}
