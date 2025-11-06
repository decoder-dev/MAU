-- Скрипт инициализации базы данных для системы мониторинга сетевого оборудования
-- Мурманский арктический университет

-- Создание базы данных
CREATE DATABASE IF NOT EXISTS NetworkMonitoringDB 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

USE NetworkMonitoringDB;

-- Таблица сетевых устройств
CREATE TABLE IF NOT EXISTS NetworkDevices (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(200),
    DeviceType VARCHAR(50) NOT NULL,
    IpAddress VARCHAR(45) NOT NULL UNIQUE,
    MacAddress VARCHAR(100),
    Model VARCHAR(100),
    Manufacturer VARCHAR(100),
    SerialNumber VARCHAR(50),
    FirmwareVersion VARCHAR(100),
    Location VARCHAR(100),
    Building VARCHAR(200),
    Room VARCHAR(200),
    Latitude DECIMAL(10, 8),
    Longitude DECIMAL(11, 8),
    Status INT NOT NULL DEFAULT 0,
    LastSeen DATETIME(6),
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6) NOT NULL,
    OneCId VARCHAR(50),
    OneCCode VARCHAR(50),
    INDEX idx_OneCId (OneCId),
    INDEX idx_Status (Status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Таблица истории статусов
CREATE TABLE IF NOT EXISTS DeviceStatusHistory (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    DeviceId INT NOT NULL,
    Status INT NOT NULL,
    Message VARCHAR(500),
    Timestamp DATETIME(6) NOT NULL,
    ChangedBy VARCHAR(100),
    FOREIGN KEY (DeviceId) REFERENCES NetworkDevices(Id) ON DELETE CASCADE,
    INDEX idx_DeviceId_Timestamp (DeviceId, Timestamp)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Таблица метрик устройств
CREATE TABLE IF NOT EXISTS DeviceMetrics (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    DeviceId INT NOT NULL,
    MetricType VARCHAR(50) NOT NULL,
    Value DECIMAL(18, 2),
    Unit VARCHAR(50),
    Timestamp DATETIME(6) NOT NULL,
    AdditionalData VARCHAR(500),
    FOREIGN KEY (DeviceId) REFERENCES NetworkDevices(Id) ON DELETE CASCADE,
    INDEX idx_DeviceId_MetricType_Timestamp (DeviceId, MetricType, Timestamp)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Вставка тестовых данных (опционально)
-- Примечание: DeviceType хранится как INT (enum значение)
-- 1 = Router, 2 = Switch, 3 = Phone, 4 = Modem, 5 = Computer, 6 = Server, и т.д.
INSERT INTO NetworkDevices (Name, Description, DeviceType, IpAddress, Location, Building, Room, Latitude, Longitude, Status, CreatedAt, UpdatedAt) VALUES
('Роутер Главный корпус', 'Основной роутер главного корпуса', 1, '192.168.1.1', 'Главный корпус', 'Главный корпус', '301', 68.9585, 33.0827, 0, NOW(), NOW()),
('Коммутатор 1 этаж', 'Коммутатор первого этажа', 2, '192.168.1.10', 'Главный корпус', 'Главный корпус', '101', 68.9586, 33.0828, 0, NOW(), NOW()),
('Сервер баз данных', 'Основной сервер баз данных', 6, '192.168.1.100', 'Главный корпус', 'Главный корпус', '401', 68.9587, 33.0829, 0, NOW(), NOW()),
('IP-телефон ректорат', 'IP-телефон в ректорате', 3, '192.168.1.50', 'Главный корпус', 'Главный корпус', '201', 68.9588, 33.0830, 0, NOW(), NOW()),
('Компьютер администратора', 'Рабочая станция администратора', 5, '192.168.1.25', 'Главный корпус', 'Главный корпус', '105', 68.9589, 33.0831, 0, NOW(), NOW());

