using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;

namespace NetworkMonitoringSystem.UI.Forms.Requests
{
    /// <summary>
    /// Создание заявки
    /// </summary>
    public partial class CreateRequestForm : Form
    {
        public CreateRequestForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Создание заявки";
            this.Size = new Size(600, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var btnOpen = new Button
            {
                Text = "Открыть форму создания заявки",
                Dock = DockStyle.Fill,
                Margin = new Padding(20)
            };
            btnOpen.Click += (s, e) =>
            {
                var form = new MaintenanceRequestForm();
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
