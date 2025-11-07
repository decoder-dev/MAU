using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление протоколами
    /// </summary>
    public partial class ProtocolsManagementForm : Form
    {
        private DataGridView dgvProtocols;
        private Button btnEnable;
        private Button btnDisable;

        public ProtocolsManagementForm()
        {
            InitializeComponent();
            LoadProtocols();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление протоколами";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnEnable = new Button { Text = "Включить", Location = new Point(15, 10), Width = 100 };
            btnDisable = new Button { Text = "Отключить", Location = new Point(125, 10), Width = 100 };
            panelTop.Controls.AddRange(new Control[] { btnEnable, btnDisable });

            dgvProtocols = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvProtocols.Columns.Add("Protocol", "Протокол");
            dgvProtocols.Columns.Add("Port", "Порт");
            dgvProtocols.Columns.Add("Status", "Статус");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvProtocols);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadProtocols()
        {
            try
            {
                dgvProtocols.Rows.Clear();
                dgvProtocols.Rows.Add("HTTP", "80", "Включен");
                dgvProtocols.Rows.Add("HTTPS", "443", "Включен");
                dgvProtocols.Rows.Add("SNMP", "161", "Включен");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка протоколов");
            }
        }
    }
}
