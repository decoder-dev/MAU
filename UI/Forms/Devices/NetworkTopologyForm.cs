using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Devices
{
    /// <summary>
    /// Топология сети
    /// </summary>
    public class NetworkTopologyForm : Form
    {
        private DataGridView dgvTopology;
        private Button btnRefresh;
        private Button btnGenerate;
        private Button btnExport;
        private TopologyService? _topologyService;

        public NetworkTopologyForm()
        {
            InitializeComponent();
            _topologyService = new TopologyService(Program.DbContext);
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Топология сети";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelButtons = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnGenerate = new Button { Text = "Сгенерировать", Location = new Point(15, 10), Width = 120 };
            btnGenerate.Click += BtnGenerate_Click;
            btnRefresh = new Button { Text = "Обновить", Location = new Point(145, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            btnExport = new Button { Text = "Экспорт", Location = new Point(255, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelButtons.Controls.AddRange(new Control[] { btnGenerate, btnRefresh, btnExport });

            dgvTopology = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvTopology.Columns.Add("Device", "Устройство");
            dgvTopology.Columns.Add("IpAddress", "IP адрес");
            dgvTopology.Columns.Add("ConnectedTo", "Подключено к");
            dgvTopology.Columns.Add("ConnectionType", "Тип подключения");
            dgvTopology.Columns.Add("Status", "Статус");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvTopology);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelButtons);
        }

        private async void LoadData()
        {
            try
            {
                dgvTopology.Rows.Clear();
                var devices = await DataService.GetDevicesAsync();
                foreach (var device in devices.OrderBy(d => d.Name))
                {
                    var connectedDevices = devices.Where(d => d.Building == device.Building && d.Id != device.Id).ToList();
                    var connectionType = device.DeviceType.ToString();
                    dgvTopology.Rows.Add(
                        device.Name,
                        device.IpAddress,
                        connectedDevices.Any() ? string.Join(", ", connectedDevices.Take(3).Select(d => d.Name)) : "Нет",
                        connectionType,
                        device.Status.ToString()
                    );
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка топологии");
            }
        }

        private void BtnGenerate_Click(object? sender, EventArgs e)
        {
            ErrorHandler.ShowInfo("Генерация топологии будет реализована");
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"topology_{DateTime.Now:yyyyMMdd}.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Устройство;IP адрес;Подключено к;Тип подключения;Статус");
                foreach (DataGridViewRow row in dgvTopology.Rows)
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value};{row.Cells[3].Value};{row.Cells[4].Value}");
                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Данные экспортированы");
            }
        }
    }
}
