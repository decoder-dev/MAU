using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление подключениями
    /// </summary>
    public partial class ConnectionsManagementForm : Form
    {
        private DataGridView dgvConnections;
        private Button btnRefresh;
        private Button btnDisconnect;

        public ConnectionsManagementForm()
        {
            InitializeComponent();
            LoadConnections();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление подключениями";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnRefresh = new Button { Text = "Обновить", Location = new Point(15, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadConnections();
            btnDisconnect = new Button { Text = "Отключить", Location = new Point(125, 10), Width = 100 };
            panelTop.Controls.AddRange(new Control[] { btnRefresh, btnDisconnect });

            dgvConnections = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvConnections.Columns.Add("Type", "Тип");
            dgvConnections.Columns.Add("Server", "Сервер");
            dgvConnections.Columns.Add("Status", "Статус");
            dgvConnections.Columns.Add("LastCheck", "Последняя проверка");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvConnections);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadConnections()
        {
            try
            {
                var connInfo = DatabaseConnectionService.GetConnectionInfo();
                dgvConnections.Rows.Clear();
                dgvConnections.Rows.Add(connInfo.ConnectionType, connInfo.Server, connInfo.IsConnected ? "Подключено" : "Отключено", DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка подключений");
            }
        }
    }
}
