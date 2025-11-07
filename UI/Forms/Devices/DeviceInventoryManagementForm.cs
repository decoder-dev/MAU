using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Devices
{
    /// <summary>
    /// Инвентаризация устройств
    /// </summary>
    public partial class DeviceInventoryManagementForm : Form
    {
        private DataGridView dgvInventory;
        private Button btnRefresh;
        private Button btnExport;
        private InventoryService? _inventoryService;

        public DeviceInventoryManagementForm()
        {
            InitializeComponent();
            _inventoryService = new InventoryService(Program.DbContext);
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Инвентаризация устройств";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelButtons = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnRefresh = new Button { Text = "Обновить", Location = new Point(15, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            btnExport = new Button { Text = "Экспорт", Location = new Point(125, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelButtons.Controls.AddRange(new Control[] { btnRefresh, btnExport });

            dgvInventory = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvInventory.Columns.Add("Name", "Название");
            dgvInventory.Columns.Add("IpAddress", "IP адрес");
            dgvInventory.Columns.Add("Type", "Тип");
            dgvInventory.Columns.Add("Model", "Модель");
            dgvInventory.Columns.Add("Manufacturer", "Производитель");
            dgvInventory.Columns.Add("SerialNumber", "Серийный номер");
            dgvInventory.Columns.Add("Location", "Местоположение");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvInventory);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelButtons);
        }

        private async void LoadData()
        {
            try
            {
                var devices = await DataService.GetDevicesAsync();
                dgvInventory.Rows.Clear();
                foreach (var device in devices.OrderBy(d => d.Name))
                {
                    dgvInventory.Rows.Add(
                        device.Name,
                        device.IpAddress,
                        device.DeviceType.ToString(),
                        device.Model ?? "-",
                        device.Manufacturer ?? "-",
                        device.SerialNumber ?? "-",
                        device.Location ?? "-"
                    );
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка инвентаризации");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"inventory_{DateTime.Now:yyyyMMdd}.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Название;IP адрес;Тип;Модель;Производитель;Серийный номер;Местоположение");
                foreach (DataGridViewRow row in dgvInventory.Rows)
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value};{row.Cells[3].Value};{row.Cells[4].Value};{row.Cells[5].Value};{row.Cells[6].Value}");
                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Данные экспортированы");
            }
        }
    }
}
