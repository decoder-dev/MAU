using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Reports
{
    /// <summary>
    /// Отчет по логам
    /// </summary>
    public partial class LogsReportForm : Form
    {
        private DataGridView dgvData;
        private Button btnRefresh;
        private Button btnExport;
        private ComboBox cmbLogLevel;

        public LogsReportForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Отчет по логам";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelFilters = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lblLevel = new Label { Text = "Уровень:", Location = new Point(15, 15), AutoSize = true };
            cmbLogLevel = new ComboBox { Location = new Point(70, 12), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbLogLevel.Items.AddRange(new[] { "Все", "INFO", "WARNING", "ERROR" });
            cmbLogLevel.SelectedIndex = 0;
            cmbLogLevel.SelectedIndexChanged += (s, e) => LoadData();
            var btnRefresh = new Button { Text = "Обновить", Location = new Point(230, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadData();
            var btnExport = new Button { Text = "Экспорт", Location = new Point(340, 10), Width = 100 };
            btnExport.Click += BtnExport_Click;
            panelFilters.Controls.AddRange(new Control[] { lblLevel, cmbLogLevel, btnRefresh, btnExport });

            dgvData = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvData.Columns.Add("Level", "Уровень");
            dgvData.Columns.Add("Count", "Количество");
            dgvData.Columns.Add("LastEntry", "Последняя запись");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvData);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelFilters);
        }

        private void LoadData()
        {
            try
            {
                dgvData.Rows.Clear();
                dgvData.Rows.Add("INFO", 0, DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                dgvData.Rows.Add("WARNING", 0, DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                dgvData.Rows.Add("ERROR", 0, DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка отчета");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"logs_report_{DateTime.Now:yyyyMMdd}.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Уровень;Количество;Последняя запись");
                foreach (DataGridViewRow row in dgvData.Rows)
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value}");
                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Отчет экспортирован");
            }
        }
    }
}
