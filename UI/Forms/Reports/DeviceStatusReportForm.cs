using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace NetworkMonitoringSystem.UI.Forms.Reports
{
    /// <summary>
    /// Отчет по статусу устройств
    /// </summary>
    public partial class DeviceStatusReportForm : Form
    {
        private DataGridView dgvData;
        private Button btnRefresh;
        private Button btnExport;
        private Button btnGenerate;
        private TextBox txtReport;
        private ReportService? _reportService;

        public DeviceStatusReportForm()
        {
            InitializeComponent();
            _reportService = new ReportService(Program.DbContext);
        }

        private void InitializeComponent()
        {
            this.Text = "Отчет по статусу устройств";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            btnGenerate = new Button { Text = "Сгенерировать отчет", Location = new Point(15, 15), Width = 150 };
            btnGenerate.Click += BtnGenerate_Click;
            btnRefresh = new Button { Text = "Обновить", Location = new Point(175, 15), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            btnExport = new Button { Text = "Экспорт CSV", Location = new Point(285, 15), Width = 120 };
            btnExport.Click += BtnExport_Click;
            panelTop.Controls.AddRange(new Control[] { btnGenerate, btnRefresh, btnExport });

            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 300
            };

            txtReport = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 9)
            };
            splitContainer.Panel1.Controls.Add(txtReport);

            dgvData = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            dgvData.Columns.Add("Status", "Статус");
            dgvData.Columns.Add("Count", "Количество");
            dgvData.Columns.Add("Percentage", "Процент");
            splitContainer.Panel2.Controls.Add(dgvData);

            this.Controls.Add(splitContainer);
            this.Controls.Add(panelTop);
        }

        private async void BtnGenerate_Click(object? sender, EventArgs e)
        {
            try
            {
                btnGenerate.Enabled = false;
                var report = await _reportService!.GenerateDeviceStatusReportAsync();
                txtReport.Text = report;
                LoadData();
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Генерация отчета");
            }
            finally
            {
                btnGenerate.Enabled = true;
            }
        }

        private async void LoadData()
        {
            try
            {
                var devices = await DataService.GetDevicesAsync();
                var total = devices.Count;
                
                dgvData.Rows.Clear();
                
                var statuses = Enum.GetValues(typeof(DeviceStatus)).Cast<DeviceStatus>();
                foreach (var status in statuses)
                {
                    var count = devices.Count(d => d.Status == status);
                    var percentage = total > 0 ? (double)count / total * 100 : 0;
                    dgvData.Rows.Add(status.ToString(), count, $"{percentage:F1}%");
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка данных");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt|CSV файлы (*.csv)|*.csv",
                FileName = $"device_status_report_{DateTime.Now:yyyyMMdd}.txt"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (dialog.FilterIndex == 1)
                {
                    File.WriteAllText(dialog.FileName, txtReport.Text, System.Text.Encoding.UTF8);
                }
                else
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("Статус;Количество;Процент");
                    foreach (DataGridViewRow row in dgvData.Rows)
                    {
                        sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value}");
                    }
                    System.IO.File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                }
                ErrorHandler.ShowSuccess("Отчет экспортирован");
            }
        }
    }
}