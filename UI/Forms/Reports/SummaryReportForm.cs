using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Reports
{
    /// <summary>
    /// Сводный отчет
    /// </summary>
    public partial class SummaryReportForm : Form
    {
        private DataGridView dgvData;
        private Button btnRefresh;
        private Button btnGenerate;
        private Button btnExport;
        private TextBox txtReport;
        private ReportService? _reportService;

        public SummaryReportForm()
        {
            InitializeComponent();
            _reportService = new ReportService(Program.DbContext);
        }

        private void InitializeComponent()
        {
            this.Text = "Сводный отчет";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            btnGenerate = new Button { Text = "Сгенерировать", Location = new Point(15, 15), Width = 130 };
            btnGenerate.Click += BtnGenerate_Click;
            btnRefresh = new Button { Text = "Обновить", Location = new Point(155, 15), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            btnExport = new Button { Text = "Экспорт", Location = new Point(265, 15), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelTop.Controls.AddRange(new Control[] { btnGenerate, btnRefresh, btnExport });

            var splitContainer = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 300 };
            txtReport = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, Font = new Font("Consolas", 9) };
            splitContainer.Panel1.Controls.Add(txtReport);

            dgvData = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvData.Columns.Add("Section", "Раздел");
            dgvData.Columns.Add("Value", "Значение");
            splitContainer.Panel2.Controls.Add(dgvData);

            this.Controls.Add(splitContainer);
            this.Controls.Add(panelTop);
        }

        private async void BtnGenerate_Click(object? sender, EventArgs e)
        {
            try
            {
                btnGenerate.Enabled = false;
                var deviceReport = await _reportService!.GenerateDeviceStatusReportAsync();
                var statsReport = await _reportService!.GenerateStatisticsReportAsync();
                txtReport.Text = $"СВОДНЫЙ ОТЧЕТ\n{new string('=', 50)}\n\n{deviceReport}\n\n{new string('=', 50)}\n\n{statsReport}";
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
                var stats = await new StatisticsService(Program.DbContext).GetSystemStatisticsAsync();
                dgvData.Rows.Clear();
                dgvData.Rows.Add("Устройства", stats.TotalDevices);
                dgvData.Rows.Add("Пользователи", stats.TotalUsers);
                dgvData.Rows.Add("Заявки", stats.TotalRequests);
                dgvData.Rows.Add("Активность", stats.TotalActivities);
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка данных");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "Текстовые файлы (*.txt)|*.txt|CSV файлы (*.csv)|*.csv", FileName = $"summary_report_{DateTime.Now:yyyyMMdd}.txt" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (dialog.FilterIndex == 1)
                    File.WriteAllText(dialog.FileName, txtReport.Text, System.Text.Encoding.UTF8);
                else
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("Раздел;Значение");
                    foreach (DataGridViewRow row in dgvData.Rows)
                        sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value}");
                    File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                }
                ErrorHandler.ShowSuccess("Отчет экспортирован");
            }
        }
    }
}
