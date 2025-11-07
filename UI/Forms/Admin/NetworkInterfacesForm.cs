using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление сетью
    /// </summary>
    public partial class NetworkInterfacesForm : Form
    {
        private DataGridView dgvNetwork;
        private Button btnRefresh;

        public NetworkInterfacesForm()
        {
            InitializeComponent();
            LoadNetwork();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление сетью";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnRefresh = new Button { Text = "Обновить", Location = new Point(15, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadNetwork();
            panelTop.Controls.Add(btnRefresh);

            dgvNetwork = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvNetwork.Columns.Add("Interface", "Интерфейс");
            dgvNetwork.Columns.Add("IP", "IP адрес");
            dgvNetwork.Columns.Add("Status", "Статус");
            dgvNetwork.Columns.Add("Speed", "Скорость");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvNetwork);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadNetwork()
        {
            try
            {
                dgvNetwork.Rows.Clear();
                var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                foreach (var iface in interfaces.Take(20))
                {
                    var props = iface.GetIPProperties();
                    var ip = props.UnicastAddresses.FirstOrDefault()?.Address?.ToString() ?? "-";
                    dgvNetwork.Rows.Add(iface.Name, iface.Description, ip, iface.OperationalStatus.ToString());
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка сети");
            }
        }
    }
}
