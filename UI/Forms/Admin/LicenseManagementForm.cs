using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление лицензиями
    /// </summary>
    public partial class LicenseManagementForm : Form
    {
        private DataGridView dgvLicenses;
        private Button btnAdd;
        private Button btnActivate;

        public LicenseManagementForm()
        {
            InitializeComponent();
            LoadLicenses();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление лицензиями";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnAdd = new Button { Text = "Добавить лицензию", Location = new Point(15, 10), Width = 130 };
            btnActivate = new Button { Text = "Активировать", Location = new Point(155, 10), Width = 120 };
            panelTop.Controls.AddRange(new Control[] { btnAdd, btnActivate });

            dgvLicenses = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvLicenses.Columns.Add("License", "Лицензия");
            dgvLicenses.Columns.Add("Type", "Тип");
            dgvLicenses.Columns.Add("Expires", "Истекает");
            dgvLicenses.Columns.Add("Status", "Статус");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvLicenses);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadLicenses()
        {
            try
            {
                dgvLicenses.Rows.Clear();
                dgvLicenses.Rows.Add("СТАНДАРТ", "Постоянная", "Неограниченно", "Активна");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка лицензий");
            }
        }
    }
}
