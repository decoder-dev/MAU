using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;

namespace NetworkMonitoringSystem.UI.Forms.Devices
{
    public partial class DeviceScanForm : Form
    {
        private TextBox txtStartIp;
        private TextBox txtEndIp;
        private TextBox txtSubnet;
        private RadioButton rbRange;
        private RadioButton rbSubnet;
        private Button btnScan;
        private Button btnImport;
        private Button btnCancel;
        private DataGridView dgvResults;
        private ProgressBar progressBar;
        private Label lblStatus;
        private List<ScanService.ScanResult>? scanResults;
        private ScanService? _scanService;

        public DeviceScanForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Сканирование сети";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 242, 245);

            // Панель настроек
            var panelSettings = new Panel
            {
                Dock = DockStyle.Top,
                Height = 150,
                Padding = new Padding(15)
            };

            var lblType = new Label
            {
                Text = "Тип сканирования:",
                Location = new Point(15, 15),
                AutoSize = true
            };

            rbRange = new RadioButton
            {
                Text = "Диапазон IP",
                Location = new Point(15, 40),
                Checked = true,
                AutoSize = true
            };
            rbRange.CheckedChanged += (s, e) => UpdateScanType();

            rbSubnet = new RadioButton
            {
                Text = "Подсеть (CIDR)",
                Location = new Point(150, 40),
                AutoSize = true
            };
            rbSubnet.CheckedChanged += (s, e) => UpdateScanType();

            var lblStartIp = new Label
            {
                Text = "Начальный IP:",
                Location = new Point(15, 70),
                AutoSize = true
            };

            txtStartIp = new TextBox
            {
                Location = new Point(120, 68),
                Width = 150,
                Text = "192.168.1.1"
            };

            var lblEndIp = new Label
            {
                Text = "Конечный IP:",
                Location = new Point(290, 70),
                AutoSize = true
            };

            txtEndIp = new TextBox
            {
                Location = new Point(370, 68),
                Width = 150,
                Text = "192.168.1.254"
            };

            var lblSubnet = new Label
            {
                Text = "Подсеть:",
                Location = new Point(15, 100),
                AutoSize = true,
                Visible = false
            };

            txtSubnet = new TextBox
            {
                Location = new Point(120, 98),
                Width = 200,
                Text = "192.168.1.0/24",
                Visible = false
            };

            btnScan = new Button
            {
                Text = "Начать сканирование",
                Location = new Point(540, 68),
                Width = 150,
                Height = 30
            };
            btnScan.Click += BtnScan_Click;

            panelSettings.Controls.AddRange(new Control[] 
            { 
                lblType, rbRange, rbSubnet, 
                lblStartIp, txtStartIp, lblEndIp, txtEndIp,
                lblSubnet, txtSubnet, btnScan 
            });

            // Результаты
            var panelResults = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            var lblResults = new Label
            {
                Text = "Результаты сканирования:",
                Dock = DockStyle.Top,
                Height = 20
            };

            dgvResults = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            dgvResults.Columns.Add("IpAddress", "IP адрес");
            dgvResults.Columns.Add("IsOnline", "Online");
            dgvResults.Columns.Add("ResponseTime", "Время ответа (мс)");
            dgvResults.Columns.Add("HostName", "Имя хоста");
            dgvResults.Columns.Add("MacAddress", "MAC адрес");

            panelResults.Controls.Add(dgvResults);
            panelResults.Controls.Add(lblResults);

            // Статус
            var panelStatus = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(15)
            };

            lblStatus = new Label
            {
                Text = "Готов к сканированию",
                Dock = DockStyle.Top,
                Height = 20
            };

            progressBar = new ProgressBar
            {
                Dock = DockStyle.Top,
                Height = 25,
                Style = ProgressBarStyle.Continuous
            };

            panelStatus.Controls.Add(progressBar);
            panelStatus.Controls.Add(lblStatus);

            // Кнопки
            var panelButtons = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(15)
            };

            btnImport = new Button
            {
                Text = "Импортировать выбранные",
                Dock = DockStyle.Right,
                Width = 180,
                Height = 30,
                Enabled = false
            };
            btnImport.Click += BtnImport_Click;

            btnCancel = new Button
            {
                Text = "Закрыть",
                Dock = DockStyle.Right,
                Width = 100,
                Height = 30,
                Margin = new Padding(0, 0, 10, 0)
            };
            btnCancel.Click += (s, e) => this.Close();

            panelButtons.Controls.AddRange(new Control[] { btnImport, btnCancel });

            this.Controls.Add(panelResults);
            this.Controls.Add(panelStatus);
            this.Controls.Add(panelSettings);
            this.Controls.Add(panelButtons);

            UpdateScanType();
        }

        private void UpdateScanType()
        {
            var isRange = rbRange.Checked;
            txtStartIp.Visible = isRange;
            txtEndIp.Visible = isRange;
            txtSubnet.Visible = !isRange;

            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Panel panel)
                {
                    foreach (Control ctrl2 in panel.Controls)
                    {
                        if ((ctrl2 == txtStartIp || ctrl2 == txtEndIp) && ctrl2 is Label)
                            ctrl2.Visible = isRange;
                        if (ctrl2 == txtSubnet && ctrl2 is Label)
                            ctrl2.Visible = !isRange;
                    }
                }
            }
        }

        private async void BtnScan_Click(object? sender, EventArgs e)
        {
            btnScan.Enabled = false;
            progressBar.Style = ProgressBarStyle.Marquee;
            lblStatus.Text = "Сканирование в процессе...";
            dgvResults.Rows.Clear();

            try
            {
                _scanService = new ScanService(Program.DbContext);

                List<ScanService.ScanResult> results;

                if (rbRange.Checked)
                {
                    if (string.IsNullOrEmpty(txtStartIp.Text) || string.IsNullOrEmpty(txtEndIp.Text))
                    {
                        ErrorHandler.ShowError("Укажите диапазон IP адресов");
                        return;
                    }
                    results = await _scanService.ScanIpRangeAsync(txtStartIp.Text, txtEndIp.Text);
                }
                else
                {
                    if (string.IsNullOrEmpty(txtSubnet.Text))
                    {
                        ErrorHandler.ShowError("Укажите подсеть в формате CIDR (например, 192.168.1.0/24)");
                        return;
                    }
                    results = await _scanService.ScanSubnetAsync(txtSubnet.Text);
                }

                scanResults = results;
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Value = 100;

                // Отображаем результаты
                foreach (var result in results)
                {
                    dgvResults.Rows.Add(
                        result.IpAddress,
                        result.IsOnline ? "✓" : "✗",
                        result.IsOnline ? result.ResponseTime.ToString() : "-",
                        result.HostName ?? "-",
                        result.MacAddress ?? "-"
                    );
                }

                lblStatus.Text = $"Найдено устройств: {results.Count(r => r.IsOnline)} из {results.Count}";
                btnImport.Enabled = results.Any(r => r.IsOnline);

                if (results.Any(r => r.IsOnline))
                {
                    ErrorHandler.ShowInfo($"Сканирование завершено.\nНайдено устройств: {results.Count(r => r.IsOnline)}");
                }
                else
                {
                    ErrorHandler.ShowWarning("Устройства не найдены");
                }
            }
            catch (Exception ex)
            {
                progressBar.Style = ProgressBarStyle.Continuous;
                lblStatus.Text = "Ошибка сканирования";
                ErrorHandler.HandleException(ex, "Сканирование сети");
            }
            finally
            {
                btnScan.Enabled = true;
            }
        }

        private async void BtnImport_Click(object? sender, EventArgs e)
        {
            if (scanResults == null || !scanResults.Any(r => r.IsOnline))
            {
                ErrorHandler.ShowWarning("Нет устройств для импорта");
                return;
            }

            var onlineResults = scanResults.Where(r => r.IsOnline).ToList();

            try
            {
                var imported = await _scanService!.ImportScannedDevicesAsync(onlineResults);
                ErrorHandler.ShowSuccess($"Импортировано устройств: {imported}");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Импорт устройств");
            }
        }
    }
}
