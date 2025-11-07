using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки времени
    /// </summary>
    public partial class TimeSettingsForm : Form
    {
        private ComboBox cmbTimeZone;
        private ComboBox cmbDateFormat;
        private ComboBox cmbTimeFormat;
        private Button btnSave;

        public TimeSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки времени";
            this.Size = new Size(500, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            var lblTZ = new Label { Text = "Часовой пояс:", Location = new Point(20, 20), AutoSize = true };
            cmbTimeZone = new ComboBox { Location = new Point(130, 17), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTimeZone.Items.AddRange(new[] { "UTC", "Москва (UTC+3)", "Киев (UTC+2)" });
            cmbTimeZone.SelectedIndex = 1;
            var lblDateFormat = new Label { Text = "Формат даты:", Location = new Point(20, 60), AutoSize = true };
            cmbDateFormat = new ComboBox { Location = new Point(130, 57), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDateFormat.Items.AddRange(new[] { "dd.MM.yyyy", "yyyy-MM-dd", "MM/dd/yyyy" });
            cmbDateFormat.SelectedIndex = 0;
            var lblTimeFormat = new Label { Text = "Формат времени:", Location = new Point(20, 100), AutoSize = true };
            cmbTimeFormat = new ComboBox { Location = new Point(140, 97), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTimeFormat.Items.AddRange(new[] { "24 часа", "12 часов" });
            cmbTimeFormat.SelectedIndex = 0;
            btnSave = new Button { Text = "Сохранить", Location = new Point(200, 140), Width = 150 };
            btnSave.Click += BtnSave_Click;
            panel.Controls.AddRange(new Control[] { lblTZ, cmbTimeZone, lblDateFormat, cmbDateFormat, lblTimeFormat, cmbTimeFormat, btnSave });
            this.Controls.Add(panel);
        }

        private void LoadSettings()
        {
            try
            {
                var config = new ConfigurationService(Program.DbContext);
                var tz = config.GetValue<string>("DateTime_TimeZone", "Москва (UTC+3)");
                cmbTimeZone.SelectedItem = tz;
                var dateFormat = config.GetValue<string>("DateTime_DateFormat", "dd.MM.yyyy");
                cmbDateFormat.SelectedItem = dateFormat;
                var timeFormat = config.GetValue<string>("DateTime_TimeFormat", "24 часа");
                cmbTimeFormat.SelectedItem = timeFormat;
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
                config.SetValue("DateTime_TimeZone", cmbTimeZone.SelectedItem?.ToString() ?? "Москва (UTC+3)");
                config.SetValue("DateTime_DateFormat", cmbDateFormat.SelectedItem?.ToString() ?? "dd.MM.yyyy");
                config.SetValue("DateTime_TimeFormat", cmbTimeFormat.SelectedItem?.ToString() ?? "24 часа");
                ErrorHandler.ShowSuccess("Настройки сохранены");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение настроек");
            }
        }
    }
}
