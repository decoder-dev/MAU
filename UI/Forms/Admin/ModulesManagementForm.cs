using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление модулями
    /// </summary>
    public partial class ModulesManagementForm : Form
    {
        private DataGridView dgvModules;
        private Button btnEnable;
        private Button btnDisable;

        public ModulesManagementForm()
        {
            InitializeComponent();
            LoadModules();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление модулями";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnEnable = new Button { Text = "Включить", Location = new Point(15, 10), Width = 100 };
            btnDisable = new Button { Text = "Отключить", Location = new Point(125, 10), Width = 100 };
            panelTop.Controls.AddRange(new Control[] { btnEnable, btnDisable });

            dgvModules = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvModules.Columns.Add("Module", "Модуль");
            dgvModules.Columns.Add("Version", "Версия");
            dgvModules.Columns.Add("Status", "Статус");
            dgvModules.Columns.Add("Description", "Описание");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvModules);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadModules()
        {
            try
            {
                dgvModules.Rows.Clear();
                dgvModules.Rows.Add("Мониторинг", "1.0.0", "Включен", "Мониторинг сетевых устройств");
                dgvModules.Rows.Add("Отчеты", "1.0.0", "Включен", "Генерация отчетов");
                dgvModules.Rows.Add("Уведомления", "1.0.0", "Включен", "Отправка уведомлений");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка модулей");
            }
        }
    }
}
