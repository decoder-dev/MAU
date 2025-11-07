using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Admin
{
    /// <summary>
    /// Управление событиями
    /// </summary>
    public partial class EventsForm : Form
    {
        private DataGridView dgvEvents;
        private Button btnRefresh;
        private ComboBox cmbEventType;

        public EventsForm()
        {
            InitializeComponent();
            LoadEvents();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление событиями";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lblType = new Label { Text = "Тип события:", Location = new Point(15, 15), AutoSize = true };
            cmbEventType = new ComboBox { Location = new Point(110, 12), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbEventType.Items.Add("Все");
            cmbEventType.Items.AddRange(new[] { "INFO", "WARNING", "ERROR" });
            cmbEventType.SelectedIndex = 0;
            cmbEventType.SelectedIndexChanged += (s, e) => LoadEvents();
            btnRefresh = new Button { Text = "Обновить", Location = new Point(320, 10), Width = 100 };
            btnRefresh.Click += (s, e) => LoadEvents();
            panelTop.Controls.AddRange(new Control[] { lblType, cmbEventType, btnRefresh });

            dgvEvents = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            dgvEvents.Columns.Add("Date", "Дата");
            dgvEvents.Columns.Add("Type", "Тип");
            dgvEvents.Columns.Add("Source", "Источник");
            dgvEvents.Columns.Add("Message", "Сообщение");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvEvents);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private async void LoadEvents()
        {
            try
            {
                dgvEvents.Rows.Clear();
                var activities = await Program.DbContext.UserActivities.OrderByDescending(a => a.Timestamp).Take(100).ToListAsync();
                foreach (var activity in activities)
                    dgvEvents.Rows.Add(activity.Action, activity.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"), activity.UserId.ToString());
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка событий");
            }
        }
    }
}
