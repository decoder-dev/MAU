using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Reports
{
    /// <summary>
    /// Отчет по восстановлению
    /// </summary>
    public partial class RestoreReportForm : Form
    {
        private DataGridView dgvData;
        private Button btnRefresh;
        private Button btnExport;

        public RestoreReportForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Отчет по восстановлению";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnRefresh = new Button { Text = "Обновить", Location = new Point(15, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            btnExport = new Button { Text = "Экспорт", Location = new Point(125, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelTop.Controls.AddRange(new Control[] { btnRefresh, btnExport });

            dgvData = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvData.Columns.Add("RestoreDate", "Дата восстановления");
            dgvData.Columns.Add("BackupFile", "Файл резервной копии");
            dgvData.Columns.Add("Records", "Восстановлено записей");
            dgvData.Columns.Add("Status", "Статус");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvData);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private async void LoadData()
        {
            try
            {
                dgvData.Rows.Clear();
                var config = new ConfigurationService(Program.DbContext);
                var restoreHistory = config.GetValue<List<object>>("RestoreHistory", new List<object>());
                
                if (restoreHistory != null && restoreHistory.Count > 0)
                {
                    foreach (var item in restoreHistory.Take(50))
                    {
                        if (item is System.Text.Json.JsonElement element)
                        {
                            var date = element.GetProperty("Date").GetString() ?? "-";
                            var file = element.GetProperty("File").GetString() ?? "-";
                            var records = element.TryGetProperty("Records", out var rec) ? rec.GetInt32() : 0;
                            var status = element.TryGetProperty("Status", out var stat) ? stat.GetString() ?? "Успешно" : "Успешно";
                            dgvData.Rows.Add(date, file, records, status);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка отчета");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"restore_report_{DateTime.Now:yyyyMMdd}.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Дата восстановления;Файл резервной копии;Восстановлено записей;Статус");
                foreach (DataGridViewRow row in dgvData.Rows)
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value};{row.Cells[3].Value}");
                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Отчет экспортирован");
            }
        }
    }
}
