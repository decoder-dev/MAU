using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;
using System.Linq;

namespace NetworkMonitoringSystem.UI.Forms
{
    /// <summary>
    /// Форма детальной информации об устройстве
    /// </summary>
    public partial class DeviceDetailsForm : Form
    {
        private NetworkDevice device;

        public DeviceDetailsForm(NetworkDevice device)
        {
            this.device = device;
            InitializeComponent();
            LoadDeviceDetails();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Text = $"Детали устройства: {device.Name} - МАУ";
            this.Size = new Size(700, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.Icon = SystemIcons.Application;

            // Основной контейнер с прокруткой
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(25)
            };

            // Контейнер для контента
            var contentPanel = new TableLayoutPanel
            {
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 15
            };
            contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
            contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            scrollPanel.Controls.Add(contentPanel);
            contentPanel.Location = new Point(0, 0);

            this.Controls.Add(scrollPanel);
            this.ResumeLayout(false);
        }

        private async void LoadDeviceDetails()
        {
            try
            {
                // Загружаем полную информацию об устройстве
                var fullDevice = await Services.DataService.GetDeviceByIdAsync(device.Id);
                if (fullDevice != null)
                {
                    device = fullDevice;
                }

                // Загружаем историю статусов
                var history = ConnectionService.IsMySqlOnline()
                    ? await Program.DbContext.DeviceStatusHistory
                        .Where(h => h.DeviceId == device.Id)
                        .OrderByDescending(h => h.Timestamp)
                        .Take(20)
                        .ToListAsync()
                    : await Program.OfflineDbContext.DeviceStatusHistory
                        .Where(h => h.DeviceId == device.Id)
                        .OrderByDescending(h => h.Timestamp)
                        .Take(20)
                        .ToListAsync();

                var contentPanel = (TableLayoutPanel)((Panel)this.Controls[0]).Controls[0];
                int row = 0;

                // Заголовок
                var lblTitle = new Label
                {
                    Text = "ОСНОВНАЯ ИНФОРМАЦИЯ",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(52, 73, 94),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Margin = new Padding(0, 0, 0, 10)
                };
                contentPanel.SetColumnSpan(lblTitle, 2);
                contentPanel.Controls.Add(lblTitle, 0, row++);
                contentPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));

                // Основные поля
                AddInfoRow(contentPanel, "Название:", device.Name, ref row);
                AddInfoRow(contentPanel, "Тип устройства:", GetDeviceTypeName(device.DeviceType), ref row);
                AddInfoRow(contentPanel, "IP адрес:", device.IpAddress, ref row);
                AddInfoRow(contentPanel, "MAC адрес:", device.MacAddress ?? "-", ref row);
                AddInfoRow(contentPanel, "Статус:", GetStatusName(device.Status), ref row);
                AddInfoRow(contentPanel, "Производитель:", device.Manufacturer ?? "-", ref row);
                AddInfoRow(contentPanel, "Модель:", device.Model ?? "-", ref row);
                AddInfoRow(contentPanel, "Серийный номер:", device.SerialNumber ?? "-", ref row);
                AddInfoRow(contentPanel, "Версия прошивки:", device.FirmwareVersion ?? "-", ref row);
                AddInfoRow(contentPanel, "Описание:", device.Description ?? "-", ref row);
                AddInfoRow(contentPanel, "Местоположение:", device.Location ?? "-", ref row);
                AddInfoRow(contentPanel, "Здание:", device.Building ?? "-", ref row);
                AddInfoRow(contentPanel, "Помещение:", device.Room ?? "-", ref row);
                AddInfoRow(contentPanel, "Последний контакт:", device.LastSeen?.ToString("dd.MM.yyyy HH:mm:ss") ?? "Никогда", ref row);
                AddInfoRow(contentPanel, "Создано:", device.CreatedAt.ToString("dd.MM.yyyy HH:mm:ss"), ref row);
                AddInfoRow(contentPanel, "Обновлено:", device.UpdatedAt.ToString("dd.MM.yyyy HH:mm:ss"), ref row);

                // История статусов
                if (history.Any())
                {
                    var lblHistoryTitle = new Label
                    {
                        Text = "ИСТОРИЯ ИЗМЕНЕНИЙ СТАТУСА",
                        Font = new Font("Segoe UI", 12, FontStyle.Bold),
                        ForeColor = Color.FromArgb(52, 73, 94),
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Margin = new Padding(0, 20, 0, 10)
                    };
                    contentPanel.SetColumnSpan(lblHistoryTitle, 2);
                    contentPanel.Controls.Add(lblHistoryTitle, 0, row++);
                    contentPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));

                    foreach (var h in history)
                    {
                        var historyText = $"{h.Timestamp:dd.MM.yyyy HH:mm:ss} - {GetStatusName(h.Status)}";
                        if (!string.IsNullOrEmpty(h.Message))
                            historyText += $" ({h.Message})";
                        if (!string.IsNullOrEmpty(h.ChangedBy))
                            historyText += $" [Изменено: {h.ChangedBy}]";

                        AddInfoRow(contentPanel, "", historyText, ref row, false);
                    }
                }

                // Кнопка закрытия
                var btnClose = new Button
                {
                    Text = "Закрыть",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(149, 165, 166),
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(120, 38),
                    Dock = DockStyle.Fill,
                    Cursor = Cursors.Hand,
                    DialogResult = DialogResult.OK
                };
                btnClose.FlatAppearance.BorderSize = 0;
                btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.FromArgb(127, 140, 141);
                btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.FromArgb(149, 165, 166);
                contentPanel.SetColumnSpan(btnClose, 2);
                contentPanel.Controls.Add(btnClose, 0, row++);
                contentPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "Загрузка деталей устройства");
            }
        }

        private void AddInfoRow(TableLayoutPanel panel, string label, string value, ref int row, bool showLabel = true)
        {
            if (showLabel)
            {
                var lbl = new Label
                {
                    Text = label,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Margin = new Padding(0, 5, 10, 5),
                    ForeColor = Color.FromArgb(52, 73, 94)
                };
                panel.Controls.Add(lbl, 0, row);
            }

            var lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 5, 0, 5),
                ForeColor = Color.FromArgb(44, 62, 80),
                AutoSize = false
            };
            panel.Controls.Add(lblValue, showLabel ? 1 : 0, row);
            if (!showLabel)
                panel.SetColumnSpan(lblValue, 2);
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            row++;
        }

        private string GetDeviceTypeName(DeviceType type)
        {
            return type switch
            {
                DeviceType.Router => "Роутер",
                DeviceType.Switch => "Коммутатор",
                DeviceType.Phone => "Телефон",
                DeviceType.Modem => "Модем",
                DeviceType.Computer => "Компьютер",
                DeviceType.Server => "Сервер",
                DeviceType.AccessPoint => "Точка доступа",
                DeviceType.Printer => "Принтер",
                DeviceType.Scanner => "Сканер",
                DeviceType.Camera => "Камера",
                DeviceType.Firewall => "Межсетевой экран",
                DeviceType.Other => "Другое",
                _ => "Неизвестно"
            };
        }

        private string GetStatusName(DeviceStatus status)
        {
            return status switch
            {
                DeviceStatus.Online => "🟢 Online",
                DeviceStatus.Offline => "🔴 Offline",
                DeviceStatus.Degraded => "🟡 Degraded",
                DeviceStatus.Maintenance => "🔵 Maintenance",
                _ => "⚪ Unknown"
            };
        }
    }
}
