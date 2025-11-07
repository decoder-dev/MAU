using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки клавиатуры
    /// </summary>
    public partial class KeyboardSettingsForm : Form
    {
        private DataGridView dgvShortcuts;
        private Button btnEdit;

        public KeyboardSettingsForm()
        {
            InitializeComponent();
            LoadShortcuts();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки клавиатуры";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15) };
            btnEdit = new Button { Text = "Редактировать", Location = new Point(15, 10), Width = 120 };
            panelTop.Controls.Add(btnEdit);

            dgvShortcuts = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvShortcuts.Columns.Add("Action", "Действие");
            dgvShortcuts.Columns.Add("Shortcut", "Горячая клавиша");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvShortcuts);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private void LoadShortcuts()
        {
            try
            {
                dgvShortcuts.Rows.Clear();
                dgvShortcuts.Rows.Add("Новое устройство", "Ctrl+N");
                dgvShortcuts.Rows.Add("Сохранить", "Ctrl+S");
                dgvShortcuts.Rows.Add("Открыть", "Ctrl+O");
                dgvShortcuts.Rows.Add("Экспорт", "Ctrl+E");
                dgvShortcuts.Rows.Add("Поиск", "Ctrl+F");
                dgvShortcuts.Rows.Add("Обновить", "F5");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка горячих клавиш");
            }
        }
    }
}
