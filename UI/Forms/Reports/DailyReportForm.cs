using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Reports
{
    /// <summary>
    /// Ежедневный отчет
    /// </summary>
    public partial class DailyReportForm : Form
    {
        private DataGridView dgvData;
        private Button btnRefresh;
        private Button btnGenerate;
        private Button btnExport;
        private TextBox txtReport;
        private DateTimePicker dtpDate;
        private ReportService? _reportService;

        public DailyReportForm()
        {
            InitializeComponent();
            _reportService = new ReportService(Program.DbContext);
        }

        private void InitializeComponent()
        {
            this.Text = "Ежедневный отчет";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilters = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(15) };
            var lblDate = new Label { Text = "Дата:", Location = new Point(15, 15), AutoSize = true };
            dtpDate = new DateTimePicker { Location = new Point(60, 12), Width = 150, Format = DateTimePickerFormat.Short };
            dtpDate.Value = DateTime.Now;
            btnGenerate = new Button { Text = "Сгенерировать", Location = new Point(220, 10), Width = 130 };
            btnGenerate.Click += BtnGenerate_Click;
            btnRefresh = new Button { Text = "Обновить", Location = new Point(360, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            btnExport = new Button { Text = "Экспорт", Location = new Point(470, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelFilters.Controls.AddRange(new Control[] { lblDate, dtpDate, btnGenerate, btnRefresh, btnExport });

            var splitContainer = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 300 };
            txtReport = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, Font = new Font("Consolas", 9) };
            splitContainer.Panel1.Controls.Add(txtReport);

            dgvData = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvData.Columns.Add("Metric", "Метрика");
            dgvData.Columns.Add("Value", "Значение");
            splitContainer.Panel2.Controls.Add(dgvData);

            this.Controls.Add(splitContainer);
            this.Controls.Add(panelFilters);
        }

        private async void BtnGenerate_Click(object? sender, EventArgs e)
        {
            try
            {
                btnGenerate.Enabled = false;
                var startDate = dtpDate.Value.Date;
                var endDate = startDate.AddDays(1);
                var report = await _reportService!.GenerateStatisticsReportAsync();
                txtReport.Text = $"ЕЖЕДНЕВНЫЙ ОТЧЕТ\nДата: {startDate:dd.MM.yyyy}\n{new string('=', 50)}\n\n{report}";
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
                dgvData.Rows.Add("Всего устройств", stats.TotalDevices);
                dgvData.Rows.Add("Онлайн устройств", stats.OnlineDevices);
                dgvData.Rows.Add("Всего пользователей", stats.TotalUsers);
                dgvData.Rows.Add("Всего заявок", stats.TotalRequests);
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка данных");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "Текстовые файлы (*.txt)|*.txt|CSV файлы (*.csv)|*.csv", FileName = $"daily_report_{dtpDate.Value:yyyyMMdd}.txt" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (dialog.FilterIndex == 1)
                    File.WriteAllText(dialog.FileName, txtReport.Text, System.Text.Encoding.UTF8);
                else
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("Метрика;Значение");
                    foreach (DataGridViewRow row in dgvData.Rows)
                        sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value}");
                    File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                }
                ErrorHandler.ShowSuccess("Отчет экспортирован");
            }
        }
    }
}
