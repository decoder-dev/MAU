using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки безопасности
    /// </summary>
    public partial class SecuritySettingsForm : Form
    {
        private NumericUpDown numPasswordMinLength;
        private CheckBox chkRequireComplexPassword;
        private NumericUpDown numSessionTimeout;
        private Button btnSave;

        public SecuritySettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки безопасности";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            var lblMinLength = new Label { Text = "Минимальная длина пароля:", Location = new Point(20, 20), AutoSize = true };
            numPasswordMinLength = new NumericUpDown { Location = new Point(200, 17), Width = 100, Minimum = 6, Maximum = 32, Value = 8 };
            chkRequireComplexPassword = new CheckBox { Text = "Требовать сложный пароль", Location = new Point(20, 60), Checked = true };
            var lblTimeout = new Label { Text = "Таймаут сессии (мин):", Location = new Point(20, 100), AutoSize = true };
            numSessionTimeout = new NumericUpDown { Location = new Point(180, 97), Width = 100, Minimum = 5, Maximum = 480, Value = 30 };
            btnSave = new Button { Text = "Сохранить", Location = new Point(200, 140), Width = 150 };
            btnSave.Click += BtnSave_Click;
            panel.Controls.AddRange(new Control[] { lblMinLength, numPasswordMinLength, chkRequireComplexPassword, lblTimeout, numSessionTimeout, btnSave });
            this.Controls.Add(panel);
        }

        private void LoadSettings()
        {
            try
            {
                var config = new ConfigurationService(Program.DbContext);
                numPasswordMinLength.Value = config.GetValue<int>("Security_PasswordMinLength", 8);
                chkRequireComplexPassword.Checked = config.GetValue<bool>("Security_RequireComplexPassword", true);
                numSessionTimeout.Value = config.GetValue<int>("Security_SessionTimeout", 30);
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
                config.SetValue("Security_PasswordMinLength", (int)numPasswordMinLength.Value);
                config.SetValue("Security_RequireComplexPassword", chkRequireComplexPassword.Checked);
                config.SetValue("Security_SessionTimeout", (int)numSessionTimeout.Value);
                ErrorHandler.ShowSuccess("Настройки сохранены");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение настроек");
            }
        }
    }
}
