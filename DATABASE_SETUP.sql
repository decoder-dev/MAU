-- ============================================
-- СКРИПТ СОЗДАНИЯ БАЗЫ ДАННЫХ
-- Система мониторинга сетевого оборудования МАУ
-- ============================================

-- Создание базы данных
CREATE DATABASE IF NOT EXISTS NetworkMonitoringDB 
    CHARACTER SET utf8mb4 
    COLLATE utf8mb4_unicode_ci;

USE NetworkMonitoringDB;

-- ============================================
-- ТАБЛИЦА USERS (Пользователи)
-- ============================================
CREATE TABLE IF NOT EXISTS Users (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash CHAR(32) NOT NULL,
    FullName VARCHAR(200) NOT NULL,
    Email VARCHAR(100) NULL,
    Role TINYINT NOT NULL DEFAULT 0 COMMENT '0=User, 1=Admin',
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastLoginAt DATETIME NULL,
    CreatedBy VARCHAR(100) NULL,
    
    INDEX IX_Users_Username (Username),
    INDEX IX_Users_Email (Email),
    INDEX IX_Users_Role (Role),
    INDEX IX_Users_IsActive (IsActive),
    INDEX IX_Users_LastLoginAt (LastLoginAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- ТАБЛИЦА NETWORKDEVICES (Сетевые устройства)
-- ============================================
CREATE TABLE IF NOT EXISTS NetworkDevices (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(200) NOT NULL,
    Description VARCHAR(500) NULL,
    DeviceType TINYINT NOT NULL DEFAULT 0,
    IpAddress VARCHAR(45) NOT NULL UNIQUE COMMENT 'Поддержка IPv6',
    MacAddress VARCHAR(17) NULL,
    Model VARCHAR(100) NULL,
    Manufacturer VARCHAR(100) NULL,
    SerialNumber VARCHAR(100) NULL,
    FirmwareVersion VARCHAR(50) NULL,
    Location VARCHAR(200) NULL,
    Building VARCHAR(200) NULL,
    Room VARCHAR(100) NULL,
    Latitude DECIMAL(10,8) NULL,
    Longitude DECIMAL(11,8) NULL,
    Status TINYINT NOT NULL DEFAULT 0,
    LastSeen DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    OneCId VARCHAR(50) NULL,
    OneCCode VARCHAR(50) NULL,
    CreatedBy VARCHAR(100) NULL,
    UpdatedBy VARCHAR(100) NULL,
    
    INDEX IX_NetworkDevices_IpAddress (IpAddress),
    INDEX IX_NetworkDevices_OneCId (OneCId),
    INDEX IX_NetworkDevices_Status (Status),
    INDEX IX_NetworkDevices_DeviceType (DeviceType),
    INDEX IX_NetworkDevices_Building_Room (Building, Room),
    INDEX IX_NetworkDevices_LastSeen (LastSeen),
    INDEX IX_NetworkDevices_CreatedAt (CreatedAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- ТАБЛИЦА DEVICESTATUSHISTORY (История статусов)
-- ============================================
CREATE TABLE IF NOT EXISTS DeviceStatusHistory (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    DeviceId INT NOT NULL,
    Status TINYINT NOT NULL,
    Message VARCHAR(500) NULL,
    Timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ChangedBy VARCHAR(100) NULL,
    
    INDEX IX_DeviceStatusHistory_DeviceId_Timestamp (DeviceId, Timestamp),
    INDEX IX_DeviceStatusHistory_Status (Status),
    INDEX IX_DeviceStatusHistory_Timestamp (Timestamp),
    
    CONSTRAINT FK_DeviceStatusHistory_NetworkDevices 
        FOREIGN KEY (DeviceId) REFERENCES NetworkDevices(Id) 
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- ТАБЛИЦА DEVICEMETRICS (Метрики устройств)
-- ============================================
CREATE TABLE IF NOT EXISTS DeviceMetrics (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    DeviceId INT NOT NULL,
    MetricType VARCHAR(50) NOT NULL,
    Value DECIMAL(10,2) NOT NULL,
    Unit VARCHAR(20) NULL,
    Timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    INDEX IX_DeviceMetrics_DeviceId_Type_Timestamp (DeviceId, MetricType, Timestamp),
    INDEX IX_DeviceMetrics_Timestamp (Timestamp),
    INDEX IX_DeviceMetrics_MetricType (MetricType),
    
    CONSTRAINT FK_DeviceMetrics_NetworkDevices 
        FOREIGN KEY (DeviceId) REFERENCES NetworkDevices(Id) 
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- ТАБЛИЦА MAINTENANCEREQUESTS (Заявки на обслуживание)
-- ============================================
CREATE TABLE IF NOT EXISTS MaintenanceRequests (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    DeviceId INT NOT NULL,
    UserId INT NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Description TEXT NULL,
    Status TINYINT NOT NULL DEFAULT 0 COMMENT '0=Pending, 1=InProgress, 2=Completed, 3=Cancelled',
    Priority TINYINT NOT NULL DEFAULT 1 COMMENT '1-4',
    AssignedTo VARCHAR(200) NULL,
    Resolution TEXT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    CompletedAt DATETIME NULL,
    
    INDEX IX_MaintenanceRequests_DeviceId_Status (DeviceId, Status),
    INDEX IX_MaintenanceRequests_UserId_Status (UserId, Status),
    INDEX IX_MaintenanceRequests_Status (Status),
    INDEX IX_MaintenanceRequests_Priority (Priority),
    INDEX IX_MaintenanceRequests_CreatedAt (CreatedAt),
    
    CONSTRAINT CK_MaintenanceRequests_Priority CHECK (Priority >= 1 AND Priority <= 4),
    
    CONSTRAINT FK_MaintenanceRequests_NetworkDevices 
        FOREIGN KEY (DeviceId) REFERENCES NetworkDevices(Id) 
        ON DELETE RESTRICT,
    
    CONSTRAINT FK_MaintenanceRequests_Users 
        FOREIGN KEY (UserId) REFERENCES Users(Id) 
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- ТАБЛИЦА USERACTIVITIES (История действий пользователей)
-- ============================================
CREATE TABLE IF NOT EXISTS UserActivities (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    UserId INT NOT NULL,
    Action VARCHAR(100) NOT NULL,
    EntityType VARCHAR(50) NULL,
    EntityId INT NULL,
    Details TEXT NULL,
    IpAddress VARCHAR(45) NULL,
    Timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    INDEX IX_UserActivities_UserId_Timestamp (UserId, Timestamp),
    INDEX IX_UserActivities_Action (Action),
    INDEX IX_UserActivities_Timestamp (Timestamp),
    INDEX IX_UserActivities_EntityType_EntityId (EntityType, EntityId),
    
    CONSTRAINT FK_UserActivities_Users 
        FOREIGN KEY (UserId) REFERENCES Users(Id) 
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- ВСТАВКА НАЧАЛЬНЫХ ДАННЫХ
-- ============================================

-- Создание администратора (пароль: admin, MD5: 21232f297a57a5a743894a0e4a801fc3)
INSERT INTO Users (Username, PasswordHash, FullName, Role, IsActive, CreatedBy) 
VALUES ('admin', '21232f297a57a5a743894a0e4a801fc3', 'Администратор системы', 1, 1, 'System')
ON DUPLICATE KEY UPDATE Username=Username;

-- Создание тестового пользователя (пароль: user, MD5: ee11cbb19052e40b07aac0ca060c23ee)
INSERT INTO Users (Username, PasswordHash, FullName, Role, IsActive, CreatedBy) 
VALUES ('user', 'ee11cbb19052e40b07aac0ca060c23ee', 'Тестовый пользователь', 0, 1, 'System')
ON DUPLICATE KEY UPDATE Username=Username;

-- Создание тестовых устройств
INSERT INTO NetworkDevices (Name, DeviceType, IpAddress, Status, Location, Building, Room, CreatedBy) VALUES
('Главный роутер МАУ', 0, '192.168.1.1', 1, 'Мурманск', 'Главный корпус', '101', 'System'),
('Коммутатор 1 этаж', 1, '192.168.1.10', 1, 'Мурманск', 'Главный корпус', '105', 'System'),
('Сервер БД', 5, '192.168.1.100', 1, 'Мурманск', 'Главный корпус', '200', 'System'),
('Сервер приложений', 5, '192.168.1.101', 1, 'Мурманск', 'Главный корпус', '200', 'System'),
('Компьютер приемной', 4, '192.168.1.50', 1, 'Мурманск', 'Главный корпус', '103', 'System'),
('Принтер HR', 7, '192.168.1.25', 1, 'Мурманск', 'Главный корпус', '150', 'System'),
('Точка доступа Wi-Fi', 6, '192.168.1.30', 0, 'Мурманск', 'Главный корпус', '201', 'System'),
('IP-камера вход', 9, '192.168.1.40', 1, 'Мурманск', 'Главный корпус', 'Вход', 'System')
ON DUPLICATE KEY UPDATE Name=Name;

-- ============================================
-- СОЗДАНИЕ ПОЛЬЗОВАТЕЛЯ ДЛЯ ПРИЛОЖЕНИЯ
-- ============================================
-- Создайте пользователя для приложения (замените 'your_password' на надежный пароль)
-- CREATE USER 'network_monitor'@'%' IDENTIFIED BY 'your_password';
-- GRANT ALL PRIVILEGES ON NetworkMonitoringDB.* TO 'network_monitor'@'%';
-- FLUSH PRIVILEGES;

-- ============================================
-- КОНЕЦ СКРИПТА
-- ============================================

