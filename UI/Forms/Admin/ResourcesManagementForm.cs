using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление ресурсами
    /// </summary>
    public partial class ResourcesManagementForm : Form
    {
        private DataGridView dgvResources;
        private Button btnRefresh;

        public ResourcesManagementForm()
        {
            InitializeComponent();
            LoadResources();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление ресурсами";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnRefresh = new Button { Text = "Обновить", Location = new Point(15, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadResources();
            panelTop.Controls.Add(btnRefresh);

            dgvResources = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvResources.Columns.Add("Resource", "Ресурс");
            dgvResources.Columns.Add("Used", "Использовано");
            dgvResources.Columns.Add("Available", "Доступно");
            dgvResources.Columns.Add("Percent", "Процент");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvResources);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadResources()
        {
            try
            {
                var mem = GC.GetTotalMemory(false) / 1024 / 1024;
                dgvResources.Rows.Clear();
                dgvResources.Rows.Add("Память", $"{mem} MB", "-", "-");
                dgvResources.Rows.Add("CPU", "-", "-", "-");
                dgvResources.Rows.Add("Диск", "-", "-", "-");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка ресурсов");
            }
        }
    }
}
