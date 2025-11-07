using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки интеграций
    /// </summary>
    public partial class IntegrationsSettingsForm : Form
    {
        private DataGridView dgvIntegrations;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnTest;

        public IntegrationsSettingsForm()
        {
            InitializeComponent();
            LoadIntegrations();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки интеграций";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnAdd = new Button { Text = "Добавить", Location = new Point(15, 10), Width = 100 };
            btnEdit = new Button { Text = "Редактировать", Location = new Point(125, 10), Width = 100 };
            btnTest = new Button { Text = "Тест", Location = new Point(235, 10), Width = 100 };
            panelTop.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnTest });

            dgvIntegrations = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvIntegrations.Columns.Add("Name", "Название");
            dgvIntegrations.Columns.Add("Type", "Тип");
            dgvIntegrations.Columns.Add("Status", "Статус");
            dgvIntegrations.Columns.Add("LastSync", "Последняя синхронизация");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvIntegrations);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadIntegrations()
        {
            try
            {
                dgvIntegrations.Rows.Clear();
                var config = new ConfigurationService(Program.DbContext);
                var integrations = new[] { "Email", "SMS", "SNMP", "API" };
                foreach (var integration in integrations)
                {
                    var isEnabled = config.GetValue<bool>($"Integration_{integration}_Enabled", false);
                    var lastSync = config.GetValue<DateTime?>($"Integration_{integration}_LastSync");
                    dgvIntegrations.Rows.Add(integration, integration, isEnabled ? "Активна" : "Неактивна", lastSync?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Никогда");
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка интеграций");
            }
        }
    }
}
