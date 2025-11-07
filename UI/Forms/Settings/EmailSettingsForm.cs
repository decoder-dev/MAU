using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки почты
    /// </summary>
    public partial class EmailSettingsForm : Form
    {
        private TextBox txtSmtpServer;
        private NumericUpDown numSmtpPort;
        private TextBox txtEmail;
        private TextBox txtPassword;
        private CheckBox chkUseSSL;
        private Button btnSave;
        private Button btnTest;

        public EmailSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки почты";
            this.Size = new Size(500, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            var lblServer = new Label { Text = "SMTP сервер:", Location = new Point(20, 20), AutoSize = true };
            txtSmtpServer = new TextBox { Location = new Point(120, 17), Width = 250 };
            var lblPort = new Label { Text = "Порт:", Location = new Point(20, 50), AutoSize = true };
            numSmtpPort = new NumericUpDown { Location = new Point(120, 47), Width = 100, Minimum = 1, Maximum = 65535, Value = 587 };
            var lblEmail = new Label { Text = "Email:", Location = new Point(20, 80), AutoSize = true };
            txtEmail = new TextBox { Location = new Point(120, 77), Width = 250 };
            var lblPassword = new Label { Text = "Пароль:", Location = new Point(20, 110), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(120, 107), Width = 250, UseSystemPasswordChar = true };
            chkUseSSL = new CheckBox { Text = "Использовать SSL", Location = new Point(20, 140), Checked = true };
            btnTest = new Button { Text = "Тест", Location = new Point(120, 170), Width = 100 };
            btnTest.Click += async (s, e) =>
            {
                try
                {
                    btnTest.Enabled = false;
                    var emailService = new EmailService(Program.DbContext);
                    await emailService.SendEmailAsync(txtEmail.Text, "Тестовое письмо", "Это тестовое письмо от системы мониторинга");
                    ErrorHandler.ShowSuccess("Тестовая отправка выполнена");
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleException(ex, "Тест отправки");
                }
                finally
                {
                    btnTest.Enabled = true;
                }
            };
            btnSave = new Button { Text = "Сохранить", Location = new Point(230, 170), Width = 150 };
            btnSave.Click += BtnSave_Click;
            panel.Controls.AddRange(new Control[] { lblServer, txtSmtpServer, lblPort, numSmtpPort, lblEmail, txtEmail, lblPassword, txtPassword, chkUseSSL, btnTest, btnSave });
            this.Controls.Add(panel);
        }

        private void LoadSettings()
        {
            try
            {
                var config = new ConfigurationService(Program.DbContext);
                txtSmtpServer.Text = config.GetValue<string>("Email_SmtpServer", "");
                numSmtpPort.Value = config.GetValue<int>("Email_SmtpPort", 587);
                txtEmail.Text = config.GetValue<string>("Email_Address", "");
                txtPassword.Text = config.GetValue<string>("Email_Password", "");
                chkUseSSL.Checked = config.GetValue<bool>("Email_UseSSL", true);
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
                config.SetValue("Email_SmtpServer", txtSmtpServer.Text);
                config.SetValue("Email_SmtpPort", (int)numSmtpPort.Value);
                config.SetValue("Email_Address", txtEmail.Text);
                config.SetValue("Email_Password", txtPassword.Text);
                config.SetValue("Email_UseSSL", chkUseSSL.Checked);
                ErrorHandler.ShowSuccess("Настройки сохранены");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение настроек");
            }
        }
    }
}
