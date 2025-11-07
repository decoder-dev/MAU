using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки обновлений
    /// </summary>
    public partial class UpdateSettingsForm : Form
    {
        private CheckBox chkAutoUpdate;
        private ComboBox cmbUpdateChannel;
        private Button btnCheck;
        private Button btnSave;

        public UpdateSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки обновлений";
            this.Size = new Size(500, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            chkAutoUpdate = new CheckBox { Text = "Автоматическая проверка обновлений", Location = new Point(20, 20), Checked = true };
            var lblChannel = new Label { Text = "Канал обновлений:", Location = new Point(20, 60), AutoSize = true };
            cmbUpdateChannel = new ComboBox { Location = new Point(150, 57), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbUpdateChannel.Items.AddRange(new[] { "Стабильный", "Бета", "Альфа" });
            cmbUpdateChannel.SelectedIndex = 0;
            btnCheck = new Button { Text = "Проверить обновления", Location = new Point(20, 100), Width = 150 };
            btnCheck.Click += async (s, e) =>
            {
                btnCheck.Enabled = false;
                btnCheck.Text = "Проверка...";
                await System.Threading.Tasks.Task.Delay(2000);
                btnCheck.Text = "Проверить обновления";
                btnCheck.Enabled = true;
                ErrorHandler.ShowInfo("Обновления не найдены");
            };
            btnSave = new Button { Text = "Сохранить", Location = new Point(200, 100), Width = 150 };
            btnSave.Click += BtnSave_Click;
            panel.Controls.AddRange(new Control[] { chkAutoUpdate, lblChannel, cmbUpdateChannel, btnCheck, btnSave });
            this.Controls.Add(panel);
        }

        private void LoadSettings()
        {
            try
            {
                var config = new ConfigurationService(Program.DbContext);
                chkAutoUpdate.Checked = config.GetValue<bool>("Updates_AutoCheck", true);
                var channel = config.GetValue<string>("Updates_Channel", "Стабильный");
                cmbUpdateChannel.SelectedItem = channel;
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
                config.SetValue("Updates_AutoCheck", chkAutoUpdate.Checked);
                config.SetValue("Updates_Channel", cmbUpdateChannel.SelectedItem?.ToString() ?? "Стабильный");
                ErrorHandler.ShowSuccess("Настройки сохранены");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение настроек");
            }
        }
    }
}
