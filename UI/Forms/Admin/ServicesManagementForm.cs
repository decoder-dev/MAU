using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление сервисами
    /// </summary>
    public partial class ServicesManagementForm : Form
    {
        private DataGridView dgvServices;
        private Button btnStart;
        private Button btnStop;
        private Button btnRestart;

        public ServicesManagementForm()
        {
            InitializeComponent();
            LoadServices();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление сервисами";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnStart = new Button { Text = "Запустить", Location = new Point(15, 10), Width = 100 };
            btnStop = new Button { Text = "Остановить", Location = new Point(125, 10), Width = 100 };
            btnRestart = new Button { Text = "Перезапустить", Location = new Point(235, 10), Width = 100 };
            panelTop.Controls.AddRange(new Control[] { btnStart, btnStop, btnRestart });

            dgvServices = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvServices.Columns.Add("Service", "Сервис");
            dgvServices.Columns.Add("Status", "Статус");
            dgvServices.Columns.Add("Description", "Описание");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvServices);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadServices()
        {
            try
            {
                dgvServices.Rows.Clear();
                dgvServices.Rows.Add("Синхронизация", "Работает", "Синхронизация данных");
                dgvServices.Rows.Add("Мониторинг", "Работает", "Мониторинг устройств");
                dgvServices.Rows.Add("Уведомления", "Остановлен", "Отправка уведомлений");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка сервисов");
            }
        }
    }
}
