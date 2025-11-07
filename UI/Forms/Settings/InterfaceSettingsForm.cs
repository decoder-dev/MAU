using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms.Settings
{
    /// <summary>
    /// Настройки интерфейса
    /// </summary>
    public partial class InterfaceSettingsForm : Form
    {
        private ComboBox cmbTheme;
        private CheckBox chkShowTooltips;
        private NumericUpDown numItemsPerPage;
        private Button btnSave;

        public InterfaceSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки интерфейса";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            var lblTheme = new Label { Text = "Тема:", Location = new Point(20, 20), AutoSize = true };
            cmbTheme = new ComboBox { Location = new Point(70, 17), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTheme.Items.AddRange(new[] { "Светлая", "Темная", "Системная" });
            cmbTheme.SelectedIndex = 0;
            chkShowTooltips = new CheckBox { Text = "Показывать подсказки", Location = new Point(20, 60), Checked = true };
            var lblItems = new Label { Text = "Элементов на странице:", Location = new Point(20, 100), AutoSize = true };
            numItemsPerPage = new NumericUpDown { Location = new Point(180, 97), Width = 100, Minimum = 10, Maximum = 1000, Value = 50 };
            btnSave = new Button { Text = "Сохранить", Location = new Point(200, 140), Width = 150 };
            btnSave.Click += BtnSave_Click;
            panel.Controls.AddRange(new Control[] { lblTheme, cmbTheme, chkShowTooltips, lblItems, numItemsPerPage, btnSave });
            this.Controls.Add(panel);
        }

        private void LoadSettings()
        {
            try
            {
                var config = new ConfigurationService(Program.DbContext);
                var theme = config.GetValue<string>("UI_Theme", "Светлая");
                cmbTheme.SelectedItem = theme;
                chkShowTooltips.Checked = config.GetValue<bool>("UI_ShowTooltips", true);
                numItemsPerPage.Value = config.GetValue<int>("UI_ItemsPerPage", 50);
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
                config.SetValue("UI_Theme", cmbTheme.SelectedItem?.ToString() ?? "Светлая");
                config.SetValue("UI_ShowTooltips", chkShowTooltips.Checked);
                config.SetValue("UI_ItemsPerPage", (int)numItemsPerPage.Value);
                ErrorHandler.ShowSuccess("Настройки сохранены");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Сохранение настроек");
            }
        }
    }
}
