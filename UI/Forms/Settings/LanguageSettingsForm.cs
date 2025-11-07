using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки языка
    /// </summary>
    public partial class LanguageSettingsForm : Form
    {
        private ComboBox cmbLanguage;
        private Button btnSave;

        public LanguageSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки языка";
            this.Size = new Size(400, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            var lbl = new Label { Text = "Язык интерфейса:", Location = new Point(20, 20), AutoSize = true };
            cmbLanguage = new ComboBox { Location = new Point(150, 17), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbLanguage.Items.AddRange(new[] { "Русский", "English", "Українська" });
            cmbLanguage.SelectedIndex = 0;
            btnSave = new Button { Text = "Сохранить", Location = new Point(150, 60), Width = 150 };
            btnSave.Click += BtnSave_Click;
            panel.Controls.AddRange(new Control[] { lbl, cmbLanguage, btnSave });
            this.Controls.Add(panel);
        }

        private void LoadSettings()
        {
            try
            {
                var config = new ConfigurationService(Program.DbContext);
                var lang = config.GetValue<string>("Language", "Русский");
                cmbLanguage.SelectedItem = lang;
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
                config.SetValue("Language", cmbLanguage.SelectedItem?.ToString() ?? "Русский");
                ErrorHandler.ShowSuccess("Настройки сохранены. Требуется перезапуск приложения.");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение настроек");
            }
        }
    }
}
