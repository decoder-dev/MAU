using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Reports
{
    /// <summary>
    /// Отчет по экспорту
    /// </summary>
    public partial class ExportReportForm : Form
    {
        private DataGridView dgvData;
        private Button btnRefresh;
        private Button btnExport;
        private Label lblInfo;

        public ExportReportForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Отчет по экспорту";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            btnRefresh = new Button { Text = "Обновить", Location = new Point(15, 15), Width = 120 };
            btnRefresh.Click += (s, e) => LoadData();
            btnExport = new Button { Text = "Экспорт", Location = new Point(145, 15), Width = 120 };
            btnExport.Click += BtnExport_Click;
            panelTop.Controls.AddRange(new Control[] { btnRefresh, btnExport });

            dgvData = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvData.Columns.Add("ExportType", "Тип экспорта");
            dgvData.Columns.Add("Count", "Количество");
            dgvData.Columns.Add("LastExport", "Последний экспорт");

            lblInfo = new Label { Dock = DockStyle.Bottom, Height = 30, Text = "Готово", Padding = new Padding(15, 5, 0, 0) };

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvData);
            panelMain.Controls.Add(lblInfo);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadData()
        {
            try
            {
                dgvData.Rows.Clear();
                dgvData.Rows.Add("Устройства", "CSV", DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                dgvData.Rows.Add("Заявки", "CSV", DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                dgvData.Rows.Add("Пользователи", "CSV", DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                lblInfo.Text = "Данные загружены";
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка данных");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = $"export_report_{DateTime.Now:yyyyMMdd}.csv" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Тип экспорта;Количество;Последний экспорт");
                foreach (DataGridViewRow row in dgvData.Rows)
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value}");
                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Данные экспортированы");
            }
        }
    }
}
