using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки шрифтов
    /// </summary>
    public partial class FontSettingsForm : Form
    {
        private ComboBox cmbFontFamily;
        private NumericUpDown numFontSize;
        private Button btnSave;

        public FontSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки шрифтов";
            this.Size = new Size(500, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            var lblFamily = new Label { Text = "Шрифт:", Location = new Point(20, 20), AutoSize = true };
            cmbFontFamily = new ComboBox { Location = new Point(80, 17), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var font in System.Drawing.FontFamily.Families)
                cmbFontFamily.Items.Add(font.Name);
            cmbFontFamily.SelectedItem = "Segoe UI";
            var lblSize = new Label { Text = "Размер:", Location = new Point(20, 60), AutoSize = true };
            numFontSize = new NumericUpDown { Location = new Point(80, 57), Width = 100, Minimum = 8, Maximum = 24, Value = 9 };
            btnSave = new Button { Text = "Сохранить", Location = new Point(200, 100), Width = 150 };
            btnSave.Click += BtnSave_Click;
            panel.Controls.AddRange(new Control[] { lblFamily, cmbFontFamily, lblSize, numFontSize, btnSave });
            this.Controls.Add(panel);
        }

        private void LoadSettings()
        {
            try
            {
                var config = new ConfigurationService(Program.DbContext);
                var font = config.GetValue<string>("Font_Family", "Segoe UI");
                cmbFontFamily.SelectedItem = font;
                numFontSize.Value = config.GetValue<int>("Font_Size", 9);
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
                config.SetValue("Font_Family", cmbFontFamily.SelectedItem?.ToString() ?? "Segoe UI");
                config.SetValue("Font_Size", (int)numFontSize.Value);
                ErrorHandler.ShowSuccess("Настройки сохранены");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение настроек");
            }
        }
    }
}
