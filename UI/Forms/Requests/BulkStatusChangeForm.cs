using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using NetworkMonitoringSystem.Core.Models;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms.Requests
{
    /// <summary>
    /// Массовое изменение статуса
    /// </summary>
    public partial class BulkStatusChangeForm : Form
    {
        private DataGridView dgvRequests;
        private ComboBox cmbStatus;
        private Button btnSelectAll;
        private Button btnChangeStatus;

        public BulkStatusChangeForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Массовое изменение статуса";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            var lblStatus = new Label { Text = "Новый статус:", Location = new Point(15, 15), AutoSize = true };
            cmbStatus = new ComboBox { Location = new Point(115, 12), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(Enum.GetNames(typeof(RequestStatus)));
            btnSelectAll = new Button { Text = "Выбрать все", Location = new Point(280, 10), Width = 100 };
            btnSelectAll.Click += (s, e) => { foreach (DataGridViewRow row in dgvRequests.Rows) row.Cells[0].Value = true; };
            btnChangeStatus = new Button { Text = "Изменить статус", Location = new Point(390, 10), Width = 130 };
            btnChangeStatus.Click += BtnChangeStatus_Click;
            panelTop.Controls.AddRange(new Control[] { lblStatus, cmbStatus, btnSelectAll, btnChangeStatus });

            dgvRequests = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false };
            dgvRequests.Columns.Add("Select", "Выбрать");
            dgvRequests.Columns[0].Width = 60;
            dgvRequests.Columns[0].CellTemplate = new DataGridViewCheckBoxCell();
            dgvRequests.Columns.Add("Id", "ID");
            dgvRequests.Columns.Add("Title", "Название");
            dgvRequests.Columns.Add("CurrentStatus", "Текущий статус");

            var panelMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            panelMain.Controls.Add(dgvRequests);

            this.Controls.Add(panelMain);
            this.Controls.Add(panelTop);
        }

        private async void LoadData()
        {
            try
            {
                var requests = await DataService.GetMaintenanceRequestsAsync();
                dgvRequests.Rows.Clear();
                foreach (var request in requests.OrderByDescending(r => r.CreatedAt))
                {
                    dgvRequests.Rows.Add(false, request.Id, request.Title, request.Status.ToString());
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка заявок");
            }
        }

        private async void BtnChangeStatus_Click(object? sender, EventArgs e)
        {
            if (cmbStatus.SelectedIndex < 0)
            {
                ErrorHandler.ShowWarning("Выберите новый статус");
                return;
            }

            var newStatus = (RequestStatus)Enum.Parse(typeof(RequestStatus), cmbStatus.Text);
            var selectedIds = new List<int>();

            foreach (DataGridViewRow row in dgvRequests.Rows)
            {
                if (row.Cells[0].Value is bool selected && selected)
                    selectedIds.Add((int)row.Cells[1].Value);
            }

            if (selectedIds.Count == 0)
            {
                ErrorHandler.ShowWarning("Выберите заявки");
                return;
            }

            try
            {
                var requests = await DataService.GetMaintenanceRequestsAsync();
                int changed = 0;
                foreach (var request in requests.Where(r => selectedIds.Contains(r.Id)))
                {
                    request.Status = newStatus;
                    if (newStatus == RequestStatus.Completed)
                        request.CompletedAt = DateTime.UtcNow;
                    if (await DataService.UpdateMaintenanceRequestAsync(request))
                        changed++;
                }
                ErrorHandler.ShowSuccess($"Изменено {changed} заявок");
                LoadData();
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Изменение статуса");
            }
        }
    }
}