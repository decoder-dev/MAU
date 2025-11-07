using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки темы
    /// </summary>
    public partial class ThemeSettingsForm : Form
    {
        private ComboBox cmbTheme;
        private Button btnPreview;
        private Button btnSave;

        public ThemeSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки темы";
            this.Size = new Size(500, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            var lbl = new Label { Text = "Тема:", Location = new Point(20, 20), AutoSize = true };
            cmbTheme = new ComboBox { Location = new Point(70, 17), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTheme.Items.AddRange(new[] { "Светлая", "Темная", "Системная", "Синяя", "Зеленая" });
            cmbTheme.SelectedIndex = 0;
            btnPreview = new Button { Text = "Предпросмотр", Location = new Point(70, 60), Width = 120 };
            btnSave = new Button { Text = "Сохранить", Location = new Point(200, 60), Width = 150 };
            btnSave.Click += BtnSave_Click;
            panel.Controls.AddRange(new Control[] { lbl, cmbTheme, btnPreview, btnSave });
            this.Controls.Add(panel);
        }

        private void LoadSettings()
        {
            try
            {
                var config = new ConfigurationService(Program.DbContext);
                var theme = config.GetValue<string>("Theme", "Светлая");
                cmbTheme.SelectedItem = theme;
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка настроек");
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                var config = new ConfigurationService(Program.DbContext);
                config.SetValue("Theme", cmbTheme.SelectedItem?.ToString() ?? "Светлая");
                ErrorHandler.ShowSuccess("Настройки сохранены");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение настроек");
            }
        }
    }
}
