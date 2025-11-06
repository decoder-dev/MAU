using NetworkMonitoringSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using NetworkMonitoringSystem.Services;
using NetworkMonitoringSystem.Helpers;

namespace NetworkMonitoringSystem.UI.Forms
{
    /// <summary>
    /// Форма карты устройств
    /// </summary>
    public partial class MapForm : Form
    {
        private Panel panelMap;
        private Label lblMap;
        private Button btnRefresh;
        private ListBox lstDevices;
        private Label lblInfo;

        public MapForm()
        {
            InitializeComponent();
            LoadDevices();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Text = "Карта устройств";
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            // Панель управления
            var panelControls = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.White,
                Padding = new Padding(15, 12, 15, 12)
            };

            btnRefresh = new Button
            {
                Text = "🔄 Обновить",
                Location = new Point(0, 0),
                Size = new Size(130, 32),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadDevices();
            btnRefresh.MouseEnter += (s, e) => btnRefresh.BackColor = Color.FromArgb(41, 128, 185);
            btnRefresh.MouseLeave += (s, e) => btnRefresh.BackColor = Color.FromArgb(52, 152, 219);
            ToolTipHelper.SetToolTip(btnRefresh, "Обновить список устройств на карте");

            panelControls.Controls.Add(btnRefresh);

            // Список устройств справа
            var panelRight = new Panel
            {
                Dock = DockStyle.Right,
                Width = 320,
                BackColor = Color.FromArgb(249, 250, 251),
                Padding = new Padding(10)
            };

            var lblDevicesTitle = new Label
            {
                Text = "Устройства на карте:",
                Dock = DockStyle.Top,
                Height = 35,
                Padding = new Padding(5, 10, 0, 0),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            lstDevices = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            lstDevices.SelectedIndexChanged += (s, e) => OnDeviceSelected();

            panelRight.Controls.Add(lstDevices);
            panelRight.Controls.Add(lblDevicesTitle);

            // Панель карты
            panelMap = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 242, 245),
                Padding = new Padding(20)
            };

            lblInfo = new Label
            {
                Text = "Загрузка устройств...",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            lblMap = new Label
            {
                Text = "",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(52, 73, 94),
                Padding = new Padding(20)
            };

            panelMap.Controls.Add(lblMap);
            panelMap.Controls.Add(lblInfo);

            this.Controls.Add(panelMap);
            this.Controls.Add(panelRight);
            this.Controls.Add(panelControls);
            this.ResumeLayout(false);
        }

        private async void LoadDevices()
        {
            try
            {
                lblInfo.Text = "Загрузка устройств...";
                lblMap.Text = "";

                var devices = await Services.DataService.GetDevicesAsync();
                var devicesWithCoordinates = devices
                    .Where(d => d.Latitude.HasValue && d.Longitude.HasValue)
                    .OrderBy(d => d.Name)
                    .ToList();

                lstDevices.Items.Clear();
                foreach (var device in devicesWithCoordinates)
                {
                    var statusEmoji = device.Status switch
                    {
                        DeviceStatus.Online => "🟢",
                        DeviceStatus.Offline => "🔴",
                        DeviceStatus.Degraded => "🟡",
                        DeviceStatus.Maintenance => "🔵",
                        _ => "⚪"
                    };
                    lstDevices.Items.Add($"{statusEmoji} {device.Name} ({device.DeviceType})");
                }

                if (devicesWithCoordinates.Count == 0)
                {
                    lblInfo.Text = "Нет устройств с указанными координатами";
                    lblMap.Text = "Для отображения устройств на карте необходимо указать их координаты (широту и долготу)\nпри добавлении или редактировании устройства.\n\nКоординаты можно получить, например, через Google Maps или Яндекс.Карты.";
                }
                else
                {
                    lblInfo.Text = $"Найдено устройств на карте: {devicesWithCoordinates.Count}";
                    var mapText = "КАРТА УСТРОЙСТВ\n\n";
                    mapText += "Для полноценной работы карты рекомендуется использовать библиотеку GMap.NET или аналогичную.\n\n";
                    mapText += "Устройства с координатами:\n\n";
                    foreach (var device in devicesWithCoordinates)
                    {
                        var statusEmoji = device.Status switch
                        {
                            DeviceStatus.Online => "🟢",
                            DeviceStatus.Offline => "🔴",
                            DeviceStatus.Degraded => "🟡",
                            DeviceStatus.Maintenance => "🔵",
                            _ => "⚪"
                        };
                        mapText += $"{statusEmoji} {device.Name}\n";
                        mapText += $"   Тип: {GetDeviceTypeName(device.DeviceType)}\n";
                        mapText += $"   Координаты: {device.Latitude}, {device.Longitude}\n";
                        mapText += $"   Статус: {GetStatusName(device.Status)}\n";
                        if (!string.IsNullOrEmpty(device.Location))
                            mapText += $"   Местоположение: {device.Location}\n";
                        mapText += "\n";
                    }
                    lblMap.Text = mapText;
                }
            }
            catch (Exception ex)
            {
                Helpers.ErrorHandler.HandleException(ex, "Загрузка устройств на карте");
                lblInfo.Text = "Ошибка загрузки устройств";
            }
        }

        private void OnDeviceSelected()
        {
            // Можно добавить логику выделения устройства на карте
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
                DeviceType.Firewall => "МЭ",
                DeviceType.Other => "Другое",
                _ => "Неизвестно"
            };
        }

        private string GetStatusName(DeviceStatus status)
        {
            return status switch
            {
                DeviceStatus.Online => "Online",
                DeviceStatus.Offline => "Offline",
                DeviceStatus.Degraded => "Degraded",
                DeviceStatus.Maintenance => "Maintenance",
                _ => "Unknown"
            };
        }
    }
}
