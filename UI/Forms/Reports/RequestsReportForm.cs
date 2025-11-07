using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Reports
{
    /// <summary>
    /// Отчет по заявкам
    /// </summary>
    public partial class RequestsReportForm : Form
    {
        private DataGridView dgvData;
        private Button btnRefresh;
        private Button btnGenerate;
        private Button btnExport;
        private TextBox txtReport;
        private DateTimePicker dtpStart;
        private DateTimePicker dtpEnd;
        private ReportService? _reportService;

        public RequestsReportForm()
        {
            InitializeComponent();
            _reportService = new ReportService(Program.DbContext);
        }

        private void InitializeComponent()
        {
            this.Text = "Отчет по заявкам";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilters = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(15) };
            var lblStart = new Label { Text = "С:", Location = new Point(15, 15), AutoSize = true };
            dtpStart = new DateTimePicker { Location = new Point(40, 12), Width = 150, Format = DateTimePickerFormat.Short };
            dtpStart.Value = DateTime.Now.AddDays(-30);
            var lblEnd = new Label { Text = "По:", Location = new Point(200, 15), AutoSize = true };
            dtpEnd = new DateTimePicker { Location = new Point(230, 12), Width = 150, Format = DateTimePickerFormat.Short };
            btnGenerate = new Button { Text = "Сгенерировать", Location = new Point(400, 10), Width = 130 };
            btnGenerate.Click += BtnGenerate_Click;
            btnRefresh = new Button { Text = "Обновить", Location = new Point(540, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            btnExport = new Button { Text = "Экспорт", Location = new Point(650, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelFilters.Controls.AddRange(new Control[] { lblStart, dtpStart, lblEnd, dtpEnd, btnGenerate, btnRefresh, btnExport });

            var splitContainer = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 300 };
            txtReport = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, Font = new Font("Consolas", 9) };
            splitContainer.Panel1.Controls.Add(txtReport);

            dgvData = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvData.Columns.Add("Status", "Статус");
            dgvData.Columns.Add("Count", "Количество");
            splitContainer.Panel2.Controls.Add(dgvData);

            this.Controls.Add(splitContainer);
            this.Controls.Add(panelFilters);
        }

        private async void BtnGenerate_Click(object? sender, EventArgs e)
        {
            try
            {
                btnGenerate.Enabled = false;
                var report = await _reportService!.GenerateMaintenanceReportAsync(dtpStart.Value, dtpEnd.Value);
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
                var requests = await DataService.GetMaintenanceRequestsAsync();
                var filtered = requests.Where(r => r.CreatedAt >= dtpStart.Value && r.CreatedAt <= dtpEnd.Value.AddDays(1)).ToList();
                dgvData.Rows.Clear();
                var statuses = Enum.GetValues(typeof(RequestStatus)).Cast<RequestStatus>();
                foreach (var status in statuses)
                {
                    var count = filtered.Count(r => r.Status == status);
                    if (count > 0)
                        dgvData.Rows.Add(status.ToString(), count);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка данных");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "Текстовые файлы (*.txt)|*.txt|CSV файлы (*.csv)|*.csv", FileName = $"requests_report_{DateTime.Now:yyyyMMdd}.txt" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (dialog.FilterIndex == 1)
                    File.WriteAllText(dialog.FileName, txtReport.Text, System.Text.Encoding.UTF8);
                else
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("Статус;Количество");
                    foreach (DataGridViewRow row in dgvData.Rows)
                        sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value}");
                    File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                }
                ErrorHandler.ShowSuccess("Отчет экспортирован");
            }
        }
    }
}
