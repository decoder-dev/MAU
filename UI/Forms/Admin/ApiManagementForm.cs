using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление API
    /// </summary>
    public partial class ApiManagementForm : Form
    {
        private DataGridView dgvApis;
        private Button btnEnable;
        private Button btnDisable;

        public ApiManagementForm()
        {
            InitializeComponent();
            LoadApis();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление API";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnEnable = new Button { Text = "Включить", Location = new Point(15, 10), Width = 100 };
            btnDisable = new Button { Text = "Отключить", Location = new Point(125, 10), Width = 100 };
            panelTop.Controls.AddRange(new Control[] { btnEnable, btnDisable });

            dgvApis = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvApis.Columns.Add("API", "API");
            dgvApis.Columns.Add("Version", "Версия");
            dgvApis.Columns.Add("Status", "Статус");
            dgvApis.Columns.Add("Endpoint", "Endpoint");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvApis);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadApis()
        {
            try
            {
                dgvApis.Rows.Clear();
                dgvApis.Rows.Add("REST API", "1.0", "Включен", "/api/v1");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка API");
            }
        }
    }
}
