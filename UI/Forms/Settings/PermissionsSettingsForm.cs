using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки прав доступа
    /// </summary>
    public partial class PermissionsSettingsForm : Form
    {
        private DataGridView dgvPermissions;
        private ComboBox cmbRole;
        private Button btnSave;

        public PermissionsSettingsForm()
        {
            InitializeComponent();
            LoadPermissions();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки прав доступа";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lblRole = new Label { Text = "Роль:", Location = new Point(15, 15), AutoSize = true };
            cmbRole = new ComboBox { Location = new Point(60, 12), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRole.Items.AddRange(new[] { "Администратор", "Пользователь" });
            cmbRole.SelectedIndex = 0;
            cmbRole.SelectedIndexChanged += (s, e) => LoadPermissions();
            btnSave = new Button { Text = "Сохранить", Location = new Point(280, 10), Width = 100 };
            btnSave.Click += BtnSave_Click;
            panelTop.Controls.AddRange(new Control[] { lblRole, cmbRole, btnSave });

            dgvPermissions = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false };
            dgvPermissions.Columns.Add("Permission", "Право");
            dgvPermissions.Columns.Add("Allowed", "Разрешено");
            dgvPermissions.Columns[1].CellTemplate = new DataGridViewCheckBoxCell();

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvPermissions);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadPermissions()
        {
            try
            {
                dgvPermissions.Rows.Clear();
                dgvPermissions.Rows.Add("Управление устройствами", cmbRole.SelectedIndex == 0);
                dgvPermissions.Rows.Add("Управление заявками", true);
                dgvPermissions.Rows.Add("Просмотр отчетов", true);
                dgvPermissions.Rows.Add("Экспорт данных", cmbRole.SelectedIndex == 0);
                dgvPermissions.Rows.Add("Системные настройки", cmbRole.SelectedIndex == 0);
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка прав доступа");
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            ErrorHandler.ShowSuccess("Настройки сохранены");
        }
    }
}
