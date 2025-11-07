using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Devices
{
    /// <summary>
    /// Импорт устройств
    /// </summary>
    public partial class DeviceImportWrapperForm : Form
    {
        public DeviceImportWrapperForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Импорт устройств";
            this.Size = new Size(600, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var btnOpen = new Button
            {
                Text = "Открыть форму импорта устройств",
                Dock = DockStyle.Fill,
                Margin = new Padding(20)
            };
            btnOpen.Click += (s, e) =>
            {
                var form = new DeviceImportForm();
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
