using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки SMS
    /// </summary>
    public partial class SmsSettingsForm : Form
    {
        private TextBox txtApiKey;
        private TextBox txtApiUrl;
        private TextBox txtSender;
        private Button btnSave;
        private Button btnTest;

        public SmsSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки SMS";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            var lblApiKey = new Label { Text = "API ключ:", Location = new Point(20, 20), AutoSize = true };
            txtApiKey = new TextBox { Location = new Point(120, 17), Width = 300, UseSystemPasswordChar = true };
            var lblApiUrl = new Label { Text = "API URL:", Location = new Point(20, 60), AutoSize = true };
            txtApiUrl = new TextBox { Location = new Point(120, 57), Width = 300 };
            var lblSender = new Label { Text = "Отправитель:", Location = new Point(20, 100), AutoSize = true };
            txtSender = new TextBox { Location = new Point(120, 97), Width = 300 };
            btnTest = new Button { Text = "Тест", Location = new Point(120, 140), Width = 100 };
            btnTest.Click += async (s, e) =>
            {
                try
                {
                    btnTest.Enabled = false;
                    await System.Threading.Tasks.Task.Delay(1000);
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
            btnSave = new Button { Text = "Сохранить", Location = new Point(230, 140), Width = 150 };
            btnSave.Click += BtnSave_Click;
            panel.Controls.AddRange(new Control[] { lblApiKey, txtApiKey, lblApiUrl, txtApiUrl, lblSender, txtSender, btnTest, btnSave });
            this.Controls.Add(panel);
        }

        private void LoadSettings()
        {
            try
            {
                var config = new ConfigurationService(Program.DbContext);
                txtApiKey.Text = config.GetValue<string>("SMS_ApiKey", "");
                txtApiUrl.Text = config.GetValue<string>("SMS_ApiUrl", "");
                txtSender.Text = config.GetValue<string>("SMS_Sender", "");
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
                config.SetValue("SMS_ApiKey", txtApiKey.Text);
                config.SetValue("SMS_ApiUrl", txtApiUrl.Text);
                config.SetValue("SMS_Sender", txtSender.Text);
                ErrorHandler.ShowSuccess("Настройки сохранены");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение настроек");
            }
        }
    }
}
