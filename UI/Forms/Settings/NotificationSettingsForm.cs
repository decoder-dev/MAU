using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки уведомлений
    /// </summary>
    public partial class NotificationSettingsForm : Form
    {
        private CheckBox chkEmail;
        private CheckBox chkSms;
        private TextBox txtEmail;
        private Button btnSave;

        public NotificationSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки уведомлений";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            chkEmail = new CheckBox { Text = "Email уведомления", Location = new Point(20, 20), Checked = true };
            var lblEmail = new Label { Text = "Email:", Location = new Point(20, 50), AutoSize = true };
            txtEmail = new TextBox { Location = new Point(70, 47), Width = 300 };
            chkSms = new CheckBox { Text = "SMS уведомления", Location = new Point(20, 90) };
            btnSave = new Button { Text = "Сохранить", Location = new Point(200, 130), Width = 150 };
            btnSave.Click += BtnSave_Click;
            panel.Controls.AddRange(new Control[] { chkEmail, lblEmail, txtEmail, chkSms, btnSave });
            this.Controls.Add(panel);
        }

        private void LoadSettings()
        {
            try
            {
                var config = new ConfigurationService(Program.DbContext);
                chkEmail.Checked = config.GetValue<bool>("Notifications_Email", true);
                chkSms.Checked = config.GetValue<bool>("Notifications_SMS", false);
                txtEmail.Text = config.GetValue<string>("Notifications_EmailAddress", "");
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
                config.SetValue("Notifications_Email", chkEmail.Checked);
                config.SetValue("Notifications_SMS", chkSms.Checked);
                config.SetValue("Notifications_EmailAddress", txtEmail.Text);
                ErrorHandler.ShowSuccess("Настройки сохранены");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение настроек");
            }
        }
    }
}
