using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// О системе
    /// </summary>
    public partial class AboutSystemForm : Form
    {
        private DataGridView dgvData = null!;
        private Button btnRefresh = null!;
        private Button btnExport = null!;
        private Label lblInfo = null!;

        public AboutSystemForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "О системе";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            btnRefresh = new Button { Text = "Обновить", Location = new Point(15, 15), Width = 120 };
            btnRefresh.Click += (s, e) => LoadData();
            btnExport = new Button { Text = "Экспорт", Location = new Point(145, 15), Width = 120 };
            btnExport.Click += (s, e) => ExportData();
            panelTop.Controls.AddRange(new Control[] { btnRefresh, btnExport });

            dgvData = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            dgvData.Columns.Add("Column1", "Колонка 1");
            dgvData.Columns.Add("Column2", "Колонка 2");

            lblInfo = new Label { Dock = DockStyle.Bottom, Height = 30, Text = "Готово" };

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvData);
            panelMain.Controls.Add(lblInfo);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private async void LoadData()
        {
            try
            {
                dgvData.Columns.Clear();
                dgvData.Columns.Add("Property", "Параметр");
                dgvData.Columns.Add("Value", "Значение");

                dgvData.Rows.Clear();

                // Информация о системе
                dgvData.Rows.Add("Название системы", "Система мониторинга сетевого оборудования МАУ");
                dgvData.Rows.Add("Версия", "1.0.0");
                dgvData.Rows.Add("Платформа", ".NET 8.0");
                dgvData.Rows.Add("Дата сборки", File.GetCreationTime(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString("dd.MM.yyyy"));

                // Статистика БД
                var deviceCount = await Program.DbContext.NetworkDevices.CountAsync();
                var userCount = await Program.DbContext.Users.CountAsync();
                var requestCount = await Program.DbContext.MaintenanceRequests.CountAsync();

                dgvData.Rows.Add("Устройств в БД", deviceCount);
                dgvData.Rows.Add("Пользователей", userCount);
                dgvData.Rows.Add("Заявок на обслуживание", requestCount);

                // Информация о подключении
                var connInfo = DatabaseConnectionService.GetConnectionInfo();
                dgvData.Rows.Add("Тип подключения", connInfo.ConnectionType);
                dgvData.Rows.Add("Сервер БД", connInfo.Server);
                dgvData.Rows.Add("База данных", connInfo.Database);

                lblInfo.Text = "Информация о системе загружена";
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка данных");
            }
        }

        private void ExportData()
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "CSV файлы (*.csv)|*.csv",
                FileName = $"export_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new System.Text.StringBuilder();
                foreach (DataGridViewRow row in dgvData.Rows)
                {
                    sb.AppendLine($"{row.Cells[0].Value};{row.Cells[1].Value}");
                }
                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ErrorHandler.ShowSuccess("Данные экспортированы");
            }
        }
    }
}